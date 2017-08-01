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

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Axe.Controllers
{
    [Authorize]
    public class QuestionsController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private AxeDbContext context;

        public QuestionsController(UserManager<ApplicationUser> userManager, AxeDbContext context)
        {
            this.userManager = userManager;
            this.context = context;
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return this.userManager.GetUserAsync(HttpContext.User);
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Input(int? id = null, int? technologyId = null)
        {
            var question = await this.context.TaskQuestion                                
                .Include(q => q.Answers)
                .SingleOrDefaultAsync(q => q.Id == id);

            if (question == null)
                question = new TaskQuestion { Answers = Enumerable.Range(1, 4).Select(i => new TaskAnswer { Value = Boolean.FalseString }).ToList() };
            else
                technologyId = question.TechnologyId;

            var questionVm = new QuestionInputVm()
            {
                Id = question.Id,                
                Text = question.Text,
                Answers = question.Answers.ToList(),
                TechnologyId = technologyId,
                Technologies = new SelectList(this.context.Technology, "Id", "Name", technologyId),
            };
            return View(questionVm);
        }

        [HttpPost]
        public async Task<IActionResult> Input(int id, QuestionInputVm questionVm, string cmd = null)
        {            
            // Answers property becomes null if all answers were removed
            if (questionVm.Answers == null)
                questionVm.Answers = new List<TaskAnswer>();

            // https://stackoverflow.com/questions/37490192/modelbinding-on-model-collection            
            cmd = cmd?.Trim()?.ToLower();
            if (cmd == "add")
            {                
                // adding one more answer option
                questionVm.Answers.Add(new TaskAnswer() { Value = Boolean.FalseString });
            }
            else if (cmd == "remove")
            {
                if (questionVm.Answers.Count > 0)
                    questionVm.Answers.RemoveAt(questionVm.Answers.Count - 1);
            }
            else
            {
                // saving valid question
                if (ModelState.IsValid)
                {
                    var q = await this.context.TaskQuestion.SingleOrDefaultAsync(t => t.Id == questionVm.Id)
                            ?? new TaskQuestion();
                    q.Text = questionVm.Text;
                    q.Answers = questionVm.Answers;
                    foreach (var a in q.Answers)
                        a.Question = q;
                    if (q.AuthorId == null)
                        q.Author = await GetCurrentUserAsync();
                    q.TechnologyId = questionVm.TechnologyId.Value;

                    if (q.Id > 0)
                        this.context.Update(q);
                    else
                        this.context.Add(q);
                    await this.context.SaveChangesAsync();

                    return RedirectToAction("Index", "Technologies", new { technologyId = q.TechnologyId });
                }
            }

            questionVm.Technologies = new SelectList(this.context.Technology, "Id", "Name", questionVm.TechnologyId);
            return View(questionVm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var question = await this.context.TaskQuestion
                                    .Include(q => q.Author)
                                    .Include(q => q.Technology)
                                    .Include(q => q.Answers)
                                    .SingleOrDefaultAsync(q => q.Id == id);

            if (question == null)
                return NotFound();

            return View(question);
        }
    }
}
