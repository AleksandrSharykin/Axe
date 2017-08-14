using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Axe.Models;
using Newtonsoft.Json;

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

        public async Task<Response<ExamAttempt>> AttemptGet(Request<ExamAttempt> request)
        {
            if (request.Item.Id > 0)
            {
                if (request.CurrentUser == null)
                {
                    return this.NotFound<ExamAttempt>();
                }

                var attempt = await this.GetAttemptData(request.Item.Id);

                if (attempt != null && attempt.StudentId == request.CurrentUser.Id)
                {
                    foreach (var question in attempt.Questions.Where(q => q.TaskQuestion.Type == TaskQuestionType.SingleChoice))
                    {
                        question.SelectedAnswerId = question.AttemptAnswers.FirstOrDefault(a => a.Value == Boolean.TrueString)?.TaskAnswerId;
                    }
                    return this.Response(attempt);
                }
                else
                {
                    return this.NotFound<ExamAttempt>();
                }
            }

            int taskId = request.Item.TaskId;

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
                TechnologyId = task.TechnologyId,
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

            int questionNum = 0;
            foreach (var q in examAttempt.Questions)
            {
                q.SortNumber = questionNum;
                questionNum++;

                int answerNum = 0;
                foreach (var a in q.AttemptAnswers)
                {
                    a.SortNumber = answerNum;
                    answerNum++;
                }
            }

            // not saving results of demo tests
            // for other tests saving each attempt to enable monitoring
            if (false == task.IsDemonstration)
            {
                examAttempt.StudentId = request.CurrentUser.Id;

                this.context.Add(examAttempt);

                await this.context.SaveChangesAsync();
            }

            return this.Response(examAttempt);
        }

        public async Task<Response<ExamAttempt>> AttemptPost(Request<ExamAttempt> request)
        {
            var attemptInput = request.Item;

            ExamAttempt examAttempt = null;
            if (attemptInput.Id > 0)
            {
                examAttempt = await this.GetAttemptData(attemptInput.Id);
                if (examAttempt == null)
                {
                    return this.NotFound<ExamAttempt>();
                }

                if (false == examAttempt.IsFinished)
                {
                    // apply submitted answers
                    foreach (var question in examAttempt.Questions)
                    {
                        var questionInput = attemptInput.Questions.FirstOrDefault(q => q.TaskQuestionId == question.TaskQuestionId);
                        if (questionInput == null)
                        {
                            continue;
                        }

                        foreach (var answer in question.AttemptAnswers)
                        {
                            var answerInput = questionInput.AttemptAnswers.FirstOrDefault(a => a.TaskAnswerId == answer.TaskAnswerId);

                            if (question.TaskQuestion.Type == TaskQuestionType.SingleChoice)
                            {
                                answerInput.Value = (answer.TaskAnswerId == questionInput.SelectedAnswerId) ? Boolean.TrueString : Boolean.FalseString;
                            }

                            if (answerInput == null)
                            {
                                continue;
                            }

                            answer.Value = answerInput.Value;
                        }
                    }

                    examAttempt.IsFinished = attemptInput.IsFinished;

                    if (examAttempt.IsFinished)
                    {
                        // mark correct answers and calculate score
                        examEvaluator.Evaluate(examAttempt);
                    }

                    this.context.Update(examAttempt);
                    await this.context.SaveChangesAsync();
                }
            }
            else
            {
                if (attemptInput.StudentId == null)
                {
                    attemptInput.Student = request.CurrentUser ?? new ApplicationUser { UserName = "Guest" };
                }

                if (attemptInput.ExamDate is null)
                {
                    attemptInput.ExamDate = DateTime.Now;
                }

                foreach (var q in attemptInput.Questions)
                {
                    q.Attempt = attemptInput;
                    foreach (var a in q.AttemptAnswers)
                    {
                        a.AttemptQuestion = q;
                    }
                }

                // restore Task properties
                var task = await this.context.ExamTask
                                     .Include(t => t.Technology)
                                     .Include(t => t.Questions).ThenInclude(q => q.Question).ThenInclude(q => q.Answers)
                                     .SingleOrDefaultAsync(t => t.Id == attemptInput.TaskId);

                attemptInput.Technology = task.Technology;
                attemptInput.Task = task;

                // restore Questions ans Answers navigation properties 
                foreach (var question in attemptInput.Questions)
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
                examEvaluator.Evaluate(attemptInput);
            }

            return this.Response(examAttempt ?? attemptInput);
        }

        /// <summary>
        /// Gets <see cref="ExamAttempt"/> object with requested identifier
        /// </summary>
        /// <param name="id">Attempt identifier</param>
        /// <returns></returns>
        private async Task<ExamAttempt> GetAttemptData(int id)
        {
            var attempt = await this.context.ExamAttempt
                                    .Include(a => a.Task)
                                    .Include(a => a.Student)
                                    .Include(a => a.Technology)
                                    .Include(a => a.Questions).ThenInclude(q => q.TaskQuestion)
                                    .Include(a => a.Questions).ThenInclude(q => q.AttemptAnswers).ThenInclude(a => a.TaskAnswer)
                                    .FirstOrDefaultAsync(a => a.Id == id);

            if (attempt != null)
            {
                attempt.Questions = attempt.Questions.OrderBy(q => q.SortNumber).ToList();

                foreach (var question in attempt.Questions.Where(q => q.AttemptAnswers.Count > 1))
                {
                    question.AttemptAnswers = question.AttemptAnswers.OrderBy(a => a.SortNumber).ToList();
                }
            }

            return attempt;
        }

        /// <summary>
        /// Loads exam attempt evaluation results
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Response<ExamAttempt>> Results(Request<int> request)
        {
            if (request.CurrentUser == null)
            {
                return this.NotFound<ExamAttempt>();
            }

            var attempt = await GetAttemptData(request.Item);

            if (attempt == null)
            {
                return this.NotFound<ExamAttempt>();
            }

            // while exam attempt is not finished, student should not be allowed to see results
            // experts can see intermediate results from attempts monitor
            if (false == attempt.IsFinished)
            {
                ApplicationUser user = request.CurrentUser;
                if (user != null)
                {
                    user = await this.context.Users.Include(u => u.Technologies)
                                             .FirstOrDefaultAsync(u => u.Id == user.Id);

                    if (false == user.Technologies.Any(t => t.TechnologyId == attempt.TechnologyId))
                    {
                        return this.NotFound<ExamAttempt>();
                    }
                }
            }

            return this.Response(attempt);
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
