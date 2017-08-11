using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Axe.Models;
using Axe.Models.QuestionsVm;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Axe.Managers
{
    /// <summary>
    /// Class implements operations governed by <see cref="Axe.Controllers.QuestionsController"/>
    /// </summary>
    public class QuestionManager : ManagerBase, IQuestionManager
    {
        public QuestionManager(AxeDbContext context) : base(context) { }

        /// <summary>
        /// Gets <see cref="TaskQuestion"/> object template for creation or edit
        /// </summary>
        public async Task<Response<QuestionInputVm>> InputGet(Request<TaskQuestion> request)
        {
            var item = request.Item;

            int id = item.Id;
            int? technologyId = item.TechnologyId;
            var questionType = item.Type;

            var user = await this.context.Users
                 .Include(u => u.Technologies).ThenInclude(t => t.Technology)
                 .SingleOrDefaultAsync(u => u.Id == request.CurrentUser.Id);

            if (false == user.Technologies.Any(t => t.TechnologyId == technologyId))
            {
                return this.NotFound<QuestionInputVm>();
            }

            var question = await this.context.TaskQuestion
                                    .Include(q => q.Answers)
                                    .AsNoTracking()
                                    .SingleOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                var editorType = Enum.IsDefined(typeof(TaskQuestionType), questionType) ?
                                questionType :
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
            foreach (var q in question.Answers)
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

            return this.Response(questionVm);
        }

        /// <summary>
        /// Restores required input model properties which are lost after postback
        /// </summary>
        /// <param name="questionVm"></param>
        /// <param name="userId"></param>
        private void RestoreInputVmProperties(QuestionInputVm questionVm, string userId)
        {
            // Answers property becomes null if all answers were removed
            if (questionVm.Answers == null)
            {
                questionVm.Answers = new List<TaskAnswer> { new TaskAnswer { } };
            }

            int c = 0;
            foreach (var a in questionVm.Answers)
            {
                a.Code = c;
                c++;
            }

            questionVm.WithUserInput = questionVm.EditorType == TaskQuestionType.SingleLine || questionVm.EditorType == TaskQuestionType.MultiLine;

            var user = this.context.Users
                            .Include(u => u.Technologies).ThenInclude(t => t.Technology)
                            .Single(u => u.Id == userId);
            questionVm.Technologies = new SelectList(user.Technologies.Select(u => u.Technology).ToList(), "Id", "Name", questionVm.TechnologyId);
        }

        /// <summary>
        /// Applies <see cref="TaskQuestion"/> edit results
        /// </summary>
        public async Task<Response<QuestionInputVm>> InputPost(Request<QuestionInputVm> request)
        {
            var user = request.CurrentUser;
            var questionVm = request.Item;

            this.RestoreInputVmProperties(questionVm, user.Id);

            if (false == request.ModelState.IsValid)
            {
                return this.ValidationError(questionVm);
            }

            if (request.ModelState.IsValid)
            {
                var tech = await this.context.Technology.Include(t => t.Experts)
                    .SingleOrDefaultAsync(t => t.Id == questionVm.TechnologyId);

                if (false == tech.Experts.Any(u => u.UserId == user.Id))
                {
                    request.ModelState.AddModelError(String.Empty, "Only " + tech.Name + " experts can write questions");
                }

                switch (questionVm.EditorType)
                {
                    case TaskQuestionType.SingleChoice:
                    foreach (var a in questionVm.Answers)
                    {
                        a.IsCorrect = a.Code == questionVm.SelectedAnswer;
                    }

                    if (questionVm.Answers.Count < 2)
                    {
                        request.ModelState.AddModelError(String.Empty, "Question should have at least two options");
                    }
                    else if (false == questionVm.Answers.Any(a => a.IsCorrect))
                    {
                        request.ModelState.AddModelError(String.Empty, "Question should have an answer");
                    }
                    break;

                    case TaskQuestionType.MultiLine:
                    case TaskQuestionType.SingleLine:
                    if (String.IsNullOrWhiteSpace(questionVm.Answers[0].Text))
                    {
                        request.ModelState.AddModelError(String.Empty, "Question should have an answer");
                    }
                    else
                    {
                        questionVm.Answers[0].Value = questionVm.Answers[0].Text;
                    }
                    break;
                }
            }

            if (false == request.ModelState.IsValid)
            {
                return this.ValidationError(questionVm);
            }

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
                foreach (var answer in questionVm.Answers)
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

            return this.Response(new QuestionInputVm { TechnologyId = question.TechnologyId });
        }

        /// <summary>
        /// Add or removes answer options from a question with choice
        /// </summary>
        public Response<QuestionInputVm> ChangeAnswers(Request<QuestionInputVm> request, bool adding)
        {
            var questionVm = request.Item;

            this.RestoreInputVmProperties(questionVm, request.CurrentUser.Id);

            if (false == questionVm.WithUserInput)
            {
                if (adding)
                {
                    // adding one more answer option
                    questionVm.Answers.Add(new TaskAnswer() { Value = Boolean.FalseString });
                }
                else if (questionVm.Answers.Count > 1)
                {
                    // removing last answer option in the list
                    questionVm.Answers.RemoveAt(questionVm.Answers.Count - 1);
                }
            }

            return this.Response(questionVm);
        }

        /// <summary>
        /// Changes question type
        /// </summary>
        public Response<QuestionInputVm> ChangeQuestionType(Request<QuestionInputVm> request, TaskQuestionType etp)
        {
            var questionVm = request.Item;

            questionVm.EditorType = etp;

            this.RestoreInputVmProperties(questionVm, request.CurrentUser.Id);

            return this.Response(questionVm);
        }

        /// <summary>
        /// Gets <see cref="TaskQuestion"/> object for preview
        /// </summary>
        public async Task<Response<TaskQuestion>> DetailsGet(Request<int> request)
        {
            var question = await this.context.TaskQuestion
                        .Include(q => q.Author)
                        .Include(q => q.Technology).ThenInclude(t => t.Experts)
                        .Include(q => q.Answers)
                        .SingleOrDefaultAsync(q => q.Id == request.Item);

            if (question == null || false == question.Technology.Experts.Any(u => u.UserId == request.CurrentUser.Id))
            {
                return this.NotFound<TaskQuestion>();
            }

            return this.Response(question);
        }

        /// <summary>
        /// Gets <see cref="TaskQuestion"/> for preview before deletion
        /// </summary>
        public async Task<Response<TaskQuestion>> DeleteGet(Request<int> request)
        {
            var question = await this.context.TaskQuestion
                                     .Include(t => t.Technology).ThenInclude(t => t.Experts)
                                     .SingleOrDefaultAsync(t => t.Id == request.Item);

            if (question == null || false == question.Technology.Experts.Any(u => u.UserId == request.CurrentUser.Id))
            {
                return this.NotFound<TaskQuestion>();
            }

            return this.Response(question);
        }

        /// <summary>
        /// Deletes <see cref="TaskQuestion"/>
        /// </summary>
        public async Task<Response<TaskQuestion>> DeletePost(Request<int> request)
        {
            var question = await this.context.TaskQuestion
                                     .Include(t => t.Technology).ThenInclude(t => t.Experts)
                                     .Include(q => q.Answers)
                                     .SingleOrDefaultAsync(t => t.Id == request.Item);

            if (question == null)
            {
                return this.NotFound<TaskQuestion>();
            }

            if (false == question.Technology.Experts.Any(u => u.UserId == request.CurrentUser.Id))
            {
                request.ModelState.AddModelError(String.Empty, "Only expert can delete " + question.Technology.Name + " questions");
                return this.ValidationError(question);
            }

            this.context.Remove(question);
            await this.context.SaveChangesAsync();

            return this.Response(new TaskQuestion());
        }
    }
}
