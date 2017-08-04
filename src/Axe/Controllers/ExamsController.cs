using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Axe.Models;
using Microsoft.AspNetCore.Authorization;

namespace Axe.Controllers
{
    public class ExamsController : ControllerExt
    {
        private readonly SignInManager<ApplicationUser> signInManager;

        public ExamsController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, AxeDbContext context)
             : base(userManager, context)                
        {
            this.signInManager = signInManager;
        }

        // GET: Exams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await GetCurrentUserAsync();
            // redirect registered users to their profile page where they can select exam
            if (user != null)
                return RedirectToAction("Visit", "Profiles", new { id = user.Id, technologyId = id });        

            return RedirectToAction("Take", "Exams", new { technologyId = id });
        }

        public async Task<IActionResult> Take(int? taskId, int? technologyId = null)
        {
            var exams = this.context.ExamTask
                .Include(t => t.Technology)
                .Include(t => t.Questions).ThenInclude(q => q.Question).ThenInclude(q => q.Answers);

            ExamTask task = null;
            
            if (taskId.HasValue)
                task = await exams.SingleOrDefaultAsync(t => t.Id == taskId);

            if (task != null && 
                false == task.IsDemonstration && 
                false == this.signInManager.IsSignedIn(HttpContext.User))
            {
                // anonymous users can only try demo exams
                return NotFound();
            }
            
            if (task == null && technologyId.HasValue)
            {
                task = exams.Where(t => t.TechnologyId == technologyId && t.IsDemonstration).FirstOrDefault();
            }

            if (task == null)
            {
                return NotFound();
            }

            var examAttempt = new ExamAttempt()
            {                
                Task = task,
                TaskId = task.Id,
                Questions = task.Questions
                                .Select(q => new AttemptQuestion
                                             {
                                                 TaskQuestion = q.Question,
                                                 TaskQuestionId = q.QuestionId,
                                                 AttemptAnswers = q.Question.Answers.Select(a => new AttemptAnswer{ TaskAnswer = a, TaskAnswerId = a.Id }).ToList()
                                             })
                                .ToList()
            };

            // todo create db record for attempted test whcih are not demonstration

            return View(examAttempt);
        }

        [HttpPost]
        public async Task<IActionResult> Take(ExamAttempt attempt)
        {
            var user = await GetCurrentUserAsync();

            if (ModelState.IsValid)
            {
                if (attempt.StudentId == null)
                    attempt.Student = user;
                if (attempt.ExamDate is null)
                    attempt.ExamDate = DateTime.Now;
                
                foreach (var q in attempt.Questions)
                {
                    q.Attempt = attempt;
                    foreach (var a in q.AttemptAnswers)
                        a.AttemptQuestion = q;
                }

                var task = await this.context.ExamTask
                                     .Include(t => t.Technology)
                                     .Include(t => t.Questions).ThenInclude(q => q.Question).ThenInclude(q => q.Answers)
                                     .SingleOrDefaultAsync(t => t.Id == attempt.TaskId);

                attempt.Technology = task.Technology;

                var questions = task.Questions.Select(q => q.Question).ToList();
                attempt.MaxScore = questions.SelectMany(q => q.Answers).Sum(a => a.Score);
                attempt.ExamScore = 0;                

                var questionPairs = questions.Join(attempt.Questions, qt => qt.Id, qa => qa.TaskQuestionId,
                    (qt, qa) => new { TaskQuestion = qt, AttemptQuestion = qa });

                // evaluate each question
                foreach (var qp in questionPairs)
                {
                    var answerPairs = qp.TaskQuestion.Answers.Join(qp.AttemptQuestion.AttemptAnswers, ta => ta.Id, aa => aa.TaskAnswerId,
                        (ta, aa) => new { TaskAnswer = ta, AttemptAnswer = aa });

                    bool isQuestionAccepted = true;
                    int questionScore = 0;
                    // compare user answers with correct answers
                    foreach (var ap in answerPairs)
                    {
                        var attemptAnswer = ap.AttemptAnswer.Value?.ToLower() ?? String.Empty;
                        var taskAnswer = ap.TaskAnswer.Value?.ToLower() ?? String.Empty;
                        if (attemptAnswer == taskAnswer)
                        {
                            questionScore += ap.TaskAnswer.Score;
                        }
                        else
                        {
                            isQuestionAccepted = false;
                        }
                    }

                    if (isQuestionAccepted || 
                        answerPairs.Where(p => p.AttemptAnswer.IsSelected).All(p => p.TaskAnswer.IsCorrect))
                        attempt.ExamScore += questionScore;                           
                }

                attempt.IsPassed = attempt.ExamScore > 0.5 * attempt.MaxScore;
                this.context.Add(attempt);
                await this.context.SaveChangesAsync();

                return View("Result", attempt);
            }
            return View(attempt);
        }

        public async Task<IActionResult> Result(int id)
        {
            var attempt = await this.context.ExamAttempt
                .Include(a => a.Task)
                .Include(a => a.Student)
                .Include(a => a.Technology)
                .Include(a => a.Questions).ThenInclude(q => q.TaskQuestion)
                .Include(a => a.Questions).ThenInclude(q => q.AttemptAnswers).ThenInclude(a => a.TaskAnswer)
                .SingleOrDefaultAsync(a => a.Id == id);
                
            if (attempt == null)
            {
                return NotFound();
            }

            return View(attempt);
        }

        // GET: Exams/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var examAttempt = await context.ExamAttempt
                .Include(e => e.Student)
                .Include(e => e.Task)
                .Include(e => e.Technology)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (examAttempt == null)
            {
                return NotFound();
            }

            return View(examAttempt);
        }

        // POST: Exams/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var examAttempt = await context.ExamAttempt.SingleOrDefaultAsync(m => m.Id == id);
            context.ExamAttempt.Remove(examAttempt);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
