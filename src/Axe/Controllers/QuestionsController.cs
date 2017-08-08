using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Axe.Models;
using Axe.Models.QuestionsVm;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Axe.Controllers
{
    [Authorize]
    public class QuestionsController : ControllerExt
    {
        public QuestionsController(UserManager<ApplicationUser> userManager, AxeDbContext context) : base(userManager, context) { }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Input(int? id = null, int? technologyId = null, int? questionType = null)
        {
            var user = await this.GetCurrentUserAsync();
            user = await this.context.Users
                             .Include(u => u.Technologies).ThenInclude(t => t.Technology)
                             .SingleOrDefaultAsync(u => u.Id == user.Id);

            if (false == user.Technologies.Any(t => t.TechnologyId == technologyId))
            {
                return RedirectToAction("Index", "Technologies");
            }

            var question = await this.context.TaskQuestion
                                    .Include(q => q.Answers)
                                    .AsNoTracking()
                                    .SingleOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                var editorType = questionType.HasValue && Enum.IsDefined(typeof(TaskQuestionType), questionType.Value) ?
                                (TaskQuestionType)questionType.Value :
                                TaskQuestionType.MultiChoice;

                // create template for a new question
                question = new TaskQuestion { Type = editorType };

                question.Answers = question.WithUserInput ?
                    question.Answers = new List<TaskAnswer> { new TaskAnswer { Text = "?" } } :
                    question.Answers = Enumerable.Range(1, 4).Select(i => new TaskAnswer { Value = Boolean.FalseString }).ToList();                                
            }
            else
            {
                technologyId = question.TechnologyId;
            }

            int c = 0;
            foreach(var q in question.Answers)
            {
                q.Code = c;
                c++;
            }

            var questionVm = new QuestionInputVm()
            {
                Id = question.Id,
                Text = question.Text,
                Answers = question.Answers.ToList(),
                EditorType = question.Type,
                WithUserInput = question.WithUserInput,
                TechnologyId = technologyId,
                Technologies = new SelectList(user.Technologies.Select(t => t.Technology), "Id", "Name", technologyId),
            };

            if (question.Type == TaskQuestionType.SingleChoice)
            {
                questionVm.SelectedAnswer = questionVm.Answers.FirstOrDefault(a => Equals(a.Value, Boolean.TrueString))?.Code;
            }

            return View(questionVm);
        }

        [HttpPost]
        public async Task<IActionResult> Input(int id, QuestionInputVm questionVm, int? etp = null, string cmd = null)
        {
            // Answers property becomes null if all answers were removed
            if (questionVm.Answers == null)
            {
                questionVm.Answers = new List<TaskAnswer>();
            }

            int c = 0;
            foreach(var a in questionVm.Answers)
            {
                a.Code = c;
                c++;
            }

            if (etp.HasValue)
            {
                questionVm.EditorType = (TaskQuestionType)etp.Value;
            }

            questionVm.WithUserInput = questionVm.EditorType == TaskQuestionType.SingleLine || questionVm.EditorType == TaskQuestionType.MultiLine;

            // https://stackoverflow.com/questions/37490192/modelbinding-on-model-collection            
            cmd = cmd?.Trim()?.ToLower();
            if (false == questionVm.WithUserInput && cmd == "add")
            {                
                // adding one more answer option
                questionVm.Answers.Add(new TaskAnswer() { Value = Boolean.FalseString });
            }
            else if (false == questionVm.WithUserInput && cmd == "remove")
            {
                if (questionVm.Answers.Count > 0)
                    questionVm.Answers.RemoveAt(questionVm.Answers.Count - 1);
            }
            else if (cmd == "save")
            {
                ApplicationUser user = null;
                if (ModelState.IsValid)
                {
                    var tech = await this.context.Technology.Include(t => t.Experts).SingleOrDefaultAsync(t => t.Id == questionVm.TechnologyId);
                    user = await this.GetCurrentUserAsync();
                    if (false == tech.Experts.Any(u => u.UserId == user.Id))
                    {
                        ModelState.AddModelError(String.Empty, "Only " + tech.Name + " experts can write questions");
                    }

                    switch(questionVm.EditorType)
                    {
                        case TaskQuestionType.SingleChoice:
                            foreach(var a in questionVm.Answers)
                            {
                                a.IsCorrect = a.Code == questionVm.SelectedAnswer;
                            }                           

                            if (questionVm.Answers.Count < 2)
                            {
                                ModelState.AddModelError(String.Empty, "Question should have at least two options");
                            }
                            else if (false == questionVm.Answers.Any(a => a.IsCorrect))
                            {
                                ModelState.AddModelError(String.Empty, "Question should have an answer");
                            }
                            break;
                        case TaskQuestionType.MultiLine: 
                        case TaskQuestionType.SingleLine:
                            if (String.IsNullOrWhiteSpace(questionVm.Answers[0].Text))
                            {
                                ModelState.AddModelError(String.Empty, "Question should have an answer");
                            }
                            else
                            {
                                questionVm.Answers[0].Value = questionVm.Answers[0].Text;
                            }
                            break;
                    }
                }
                // saving valid question
                if (ModelState.IsValid)
                {
                    var question = await this.context.TaskQuestion
                                             .Include(q => q.Answers)
                                             .SingleOrDefaultAsync(t => t.Id == questionVm.Id);

                    if (question == null)
                    {
                        question = new TaskQuestion();
                        question.Answers = questionVm.Answers;
                        foreach (var a in question.Answers)
                            a.Question = question;
                    }
                    else
                    {
                        // add new asnwers, apply changes from modified answers
                        foreach(var answer in questionVm.Answers)
                        {
                            var original = question.Answers.SingleOrDefault(a => a.Id == answer.Id);
                            if (original == null)
                            {
                                question.Answers.Add(answer);
                                answer.Question = question;
                            }
                            else
                            {
                                original.Text = answer.Text;
                                original.Score = answer.Score;
                                original.Value = answer.Value;
                            }
                        }
                        // delete removed answers
                        var deleted = question.Answers
                                              .Where(a => false == questionVm.Answers.Any(x => x.Id == a.Id))
                                              .ToList();

                        this.context.RemoveRange(deleted);
                        foreach (var d in deleted)
                            question.Answers.Remove(d);
                    }

                    question.Text = questionVm.Text;
                    question.Type = questionVm.EditorType;

                    if (question.AuthorId == null)
                    {
                        question.Author = user;
                    }

                    question.TechnologyId = questionVm.TechnologyId.Value;

                    if (question.Id > 0)
                    {
                        this.context.Update(question);
                    }
                    else
                    {
                        this.context.Add(question);
                    }
                    
                    await this.context.SaveChangesAsync();

                    return RedirectToAction("Index", "Technologies", new { technologyId = question.TechnologyId });
                }
            }

            questionVm.Technologies = new SelectList(this.context.Technology, "Id", "Name", questionVm.TechnologyId);
            return View(questionVm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var question = await this.context.TaskQuestion
                                    .Include(q => q.Author)
                                    .Include(q => q.Technology).ThenInclude(t => t.Experts)
                                    .Include(q => q.Answers)
                                    .SingleOrDefaultAsync(q => q.Id == id);

            var user = await this.GetCurrentUserAsync();

            if (question == null || false == question.Technology.Experts.Any(u=>u.UserId == user.Id))
            {
                return NotFound();
            }

            return View(question);
        }
    }
}
