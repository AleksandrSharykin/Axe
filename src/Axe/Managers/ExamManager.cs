using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Axe.Models;

namespace Axe.Managers
{
    /// <summary>
    /// Class implements operations which can be performed with <see cref="ExamAttempt"/> entities
    /// </summary>
    public class ExamManager : ManagerBase, IExamManager
    {
        IExamEvaluator examEvaluator;

        public ExamManager(AxeDbContext context, IExamEvaluator examEvaluator) : base(context)
        {
            this.examEvaluator = examEvaluator;
        }

        public async Task<Response<ExamAttempt>> AttemptGet(Request<ExamTask> request)
        {
            int taskId = request.Item.Id;

            var exams = this.context.ExamTask
                            .Include(t => t.Technology)
                            .Include(t => t.Questions).ThenInclude(q => q.Question).ThenInclude(q => q.Answers);

            ExamTask task = null;

            if (taskId > 0)
            {
                // trying to load requested exam
                task = await exams.SingleOrDefaultAsync(t => t.Id == taskId);
            }

            if (task != null &&
                false == task.IsDemonstration &&
                request.CurrentUser == null)
            {
                // anonymous users can only try demo exams
                return this.NotFound<ExamAttempt>();
            }

            int? technologyId = request.Item.TechnologyId;
            if (task == null && technologyId.HasValue)
            {
                // trying to load demo exam for selected technology
                task = exams.Where(t => t.TechnologyId == technologyId && t.IsDemonstration).FirstOrDefault();
            }

            if (task == null)
            {
                return this.NotFound<ExamAttempt>();
            }

            // prepare new test
            var examAttempt = new ExamAttempt()
            {
                ExamDate = DateTime.Now,
                Task = task,
                TaskId = task.Id,
                Questions = task.Questions
                                .Select(q => new AttemptQuestion
                                {
                                    TaskQuestion = q.Question,
                                    TaskQuestionId = q.QuestionId,
                                    AttemptAnswers = q.Question.Answers
                                                        .Select((a, i) => new AttemptAnswer { TaskAnswer = a, TaskAnswerId = a.Id, })
                                                        .ToList().Shuffle()
                                })
                                .ToList().Shuffle()
            };

            return this.Response(examAttempt);
        }

        public async Task<Response<ExamAttempt>> AttemptPost(Request<ExamAttempt> request)
        {
            var attempt = request.Item;

            if (attempt.StudentId == null)
                attempt.Student = request.CurrentUser ?? new ApplicationUser { UserName = "Guest" };

            if (attempt.ExamDate is null)
                attempt.ExamDate = DateTime.Now;

            foreach (var q in attempt.Questions)
            {
                q.Attempt = attempt;
                foreach (var a in q.AttemptAnswers)
                    a.AttemptQuestion = q;
            }

            // restore Task properties
            var task = await this.context.ExamTask
                                 .Include(t => t.Technology)
                                 .Include(t => t.Questions).ThenInclude(q => q.Question).ThenInclude(q => q.Answers)
                                 .SingleOrDefaultAsync(t => t.Id == attempt.TaskId);

            attempt.Technology = task.Technology;
            attempt.Task = task;

            // restore Questions ans Answers navigation properties 
            foreach (var question in attempt.Questions)
            {
                question.TaskQuestion = task.Questions.FirstOrDefault(q => q.QuestionId == question.TaskQuestionId).Question;
                foreach (var answer in question.AttemptAnswers)
                {
                    answer.TaskAnswer = question.TaskQuestion.Answers.FirstOrDefault(a => a.Id == answer.TaskAnswerId);
                    if (question.TaskQuestion.Type == TaskQuestionType.SingleChoice)
                    {
                        answer.Value = (answer.TaskAnswer.Id == question.SelectedAnswerId) ? Boolean.TrueString : Boolean.FalseString;
                    }
                }
            }

            // mark correct answers and calculate score
            examEvaluator.Evaluate(attempt);

            // not saving results of demo tests
            if (false == task.IsDemonstration)
            {
                this.context.Add(attempt);
                await this.context.SaveChangesAsync();
            }

            return this.Response(attempt);
        }

        public async Task<Response<ExamAttempt>> Results(Request<int> request)
        {
            var attempt = await this.context.ExamAttempt
                                    .Include(a => a.Task)
                                    .Include(a => a.Student)
                                    .Include(a => a.Technology)
                                    .Include(a => a.Questions).ThenInclude(q => q.TaskQuestion)
                                    .Include(a => a.Questions).ThenInclude(q => q.AttemptAnswers).ThenInclude(a => a.TaskAnswer)
                                    .SingleOrDefaultAsync(a => a.Id == request.Item);

            return attempt != null ? this.Response(attempt) : this.NotFound<ExamAttempt>();
        }

        /// <summary>
        /// Loads exam attempt details for preview before deletion
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Response<ExamAttempt>> DeletePreview(Request<int> request)
        {
            var attempt = await context.ExamAttempt
                .Include(e => e.Student)
                .Include(e => e.Task)
                .Include(e => e.Technology)
                .SingleOrDefaultAsync(m => m.Id == request.Item);

            if (attempt == null || attempt.StudentId != request.CurrentUser.Id)
            {
                return this.NotFound<ExamAttempt>();
            }

            return this.Response(attempt);
        }

        /// <summary>
        /// Deletes exam attempt
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Response<bool>> Delete(Request<int> request)
        {
            var attempt = await context.ExamAttempt.SingleOrDefaultAsync(m => m.Id == request.Item);

            if (attempt != null)
            {
                context.ExamAttempt.Remove(attempt);

                await context.SaveChangesAsync();

                return this.Response(true);
            }

            return this.Response(false);
        }
    }
}
