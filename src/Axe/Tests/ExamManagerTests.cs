using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Axe.Models;
using Axe.Managers;

namespace Axe.Tests
{
    public class ExamManagerTests : DbDependentTests
    {
        private AxeDbContext dbManager;
        private IExamManager manager;

        private ExamTask demoA, testA, testB;

        private static string True { get { return ExamEvaluatorTests.True; } }
        private static string False { get { return ExamEvaluatorTests.False; } }

        #region Setup
        [OneTimeSetUp]
        public void InitTestFixture()
        {
            this.InitStorage("ExamManagerDb");
            this.InitTechnologies();

            this.demoA = this.CreateTask(this.techA);
            this.demoA.IsDemonstration = true;
            this.demoA.Title = nameof(this.demoA);

            this.testA = this.CreateTask(this.techA);
            this.testA.Title = nameof(this.testA);

            this.testB = this.CreateTask(this.techB);
            this.testB.Title = nameof(this.testB);

            using (var storage = NewDbContext())
            {
                storage.AddRange(this.demoA, this.testA, this.testB);
                storage.SaveChanges();
            }
        }

        private ExamTask CreateTask(Technology tech)
        {
            var questionFirst = new TaskQuestion
            {
                TechnologyId = tech.Id,
                Type = TaskQuestionType.SingleChoice,
            };
            questionFirst.Answers = new List<TaskAnswer>
            {
                new TaskAnswer { Question = questionFirst, Value = False },
                new TaskAnswer { Question = questionFirst, Value = True },
                new TaskAnswer { Question = questionFirst, Value = False },
                new TaskAnswer { Question = questionFirst, Value = False },
            };

            var questionSecond = new TaskQuestion
            {
                TechnologyId = tech.Id,
                Type = TaskQuestionType.MultiChoice,
            };
            questionFirst.Answers = new List<TaskAnswer>
            {
                new TaskAnswer { Question = questionFirst, Value = False },
                new TaskAnswer { Question = questionFirst, Value = True },
                new TaskAnswer { Question = questionFirst, Value = False },
                new TaskAnswer { Question = questionFirst, Value = True },
            };

            var task = new ExamTask
            {
                TechnologyId = tech.Id,
            };
            task.Questions = new List<TaskQuestionLink>
            {
                new TaskQuestionLink { Question = questionFirst, Task = task},
                new TaskQuestionLink { Question = questionSecond, Task = task},
            };

            return task;
        }

        private IExamManager NewManager(AxeDbContext context)
        {
            return new ExamManager(context, new ExamEvaluator());
        }

        [SetUp]
        public void InitTestCase()
        {
            this.db = NewDbContext();

            this.dbManager = NewDbContext();
            this.manager = NewManager(this.dbManager);
        }

        [TearDown]
        public void ClearTestCase()
        {
            this.dbManager.Dispose();
            this.db.Dispose();
        }

        /// <summary>
        /// Verifies that all information properties are set
        /// </summary>
        /// <param name="exam"></param>
        /// <returns></returns>
        private bool IsAttemptDataComplete(ExamAttempt exam)
        {
            return exam.Task != null &&
                   exam.Technology != null &&
                   exam.Questions.All(q => q.TaskQuestion != null && q.AttemptAnswers.All(a => a.TaskAnswer != null));
        }

        [TestCase]
        public async Task ExamGet_AnonymousUser_TechnologyDemo()
        {
            var request = this.Request(new ExamAttempt { TechnologyId = techA.Id });

            var response = await this.manager.AttemptGet(request);

            Assert.AreEqual(ResponseCode.Success, response.Code);
            Assert.AreEqual(this.demoA.Id, response.Item.Task.Id);

            int count = this.db.ExamAttempt.Count();
            Assert.AreEqual(0, count);

            var exam = response.Item;
            Assert.True(this.IsAttemptDataComplete(exam));
            Assert.False(exam.IsFinished);
            Assert.Null(exam.IsPassed);
        }

        [TestCase]
        public async Task ExamGet_AnonymousUser_NoTechnologyDemo()
        {
            var request = this.Request(new ExamAttempt { TechnologyId = techB.Id });

            var response = await this.manager.AttemptGet(request);

            Assert.AreEqual(ResponseCode.NotFound, response.Code);
            Assert.Null(response.Item);
        }

        [TestCase]
        public async Task ExamGet_AnonymousUser_TaskDemo()
        {
            var request = this.Request(new ExamAttempt { TaskId = this.demoA.Id });

            var response = await this.manager.AttemptGet(request);

            Assert.AreEqual(ResponseCode.Success, response.Code);
            Assert.AreEqual(this.demoA.Id, response.Item.Task.Id);

            int count = this.db.ExamAttempt.Count();
            Assert.AreEqual(0, count);

            var exam = response.Item;
            Assert.True(this.IsAttemptDataComplete(exam));
        }

        [TestCase]
        public async Task ExamGet_AnonymousUser_TaskNoAccess()
        {
            var request = this.Request(new ExamAttempt { TaskId = this.testA.Id });

            var response = await this.manager.AttemptGet(request);

            Assert.AreEqual(ResponseCode.NotFound, response.Code);
            Assert.Null(response.Item);
        }

        [TestCase]
        public async Task ExamGet_AnonymousUser_AttemptId()
        {
            var request = this.Request(new ExamAttempt { Id = 1 });

            var response = await this.manager.AttemptGet(request);

            Assert.AreEqual(ResponseCode.NotFound, response.Code);
            Assert.Null(response.Item);
        }

        [TestCase]
        public async Task ExamGet_Member_NewExamAttempt()
        {
            var request = this.Request(new ExamAttempt { TaskId = this.testB.Id }, this.expertA);

            var response = await this.manager.AttemptGet(request);

            Assert.AreEqual(ResponseCode.Success, response.Code);

            var exam = response.Item;

            Assert.NotNull(exam);
            Assert.True(this.IsAttemptDataComplete(exam));
            Assert.True(exam.Id > 0);
            Assert.False(exam.IsFinished);
            Assert.Null(exam.IsPassed);

            int count = this.db.ExamAttempt.Count(a => a.Id == exam.Id);
            Assert.AreEqual(1, count);


            this.dbManager.Remove(exam);
            this.dbManager.SaveChanges();
        }

        [TestCase]
        public async Task ExamGet_Member_AttemptById()
        {
            var newAttemptRequest = this.Request(new ExamAttempt { TaskId = this.testB.Id }, this.expertA);

            var newAttemptResponse = await this.manager.AttemptGet(newAttemptRequest);

            var exam = newAttemptResponse.Item;
            Assert.AreEqual(ResponseCode.Success, newAttemptResponse.Code);
            Assert.NotNull(exam);
            Assert.True(this.IsAttemptDataComplete(exam));

            // users can reload their own atttempts
            var reloadRequest = this.Request(new ExamAttempt { Id = exam.Id }, this.expertA);
            var reloadResponse = await this.manager.AttemptGet(reloadRequest);

            Assert.AreEqual(ResponseCode.Success, reloadResponse.Code);
            Assert.NotNull(reloadResponse.Item);
            Assert.True(this.IsAttemptDataComplete(reloadResponse.Item));
            Assert.AreEqual(exam.Id, reloadResponse.Item.Id);

            // cannot load attempts of other users
            reloadRequest = this.Request(new ExamAttempt { Id = exam.Id }, this.expertB);
            reloadResponse = await this.manager.AttemptGet(reloadRequest);

            Assert.AreEqual(ResponseCode.NotFound, reloadResponse.Code);
            Assert.Null(reloadResponse.Item);

            this.dbManager.Remove(exam);
            this.dbManager.SaveChanges();
        }

        [TestCase]
        public async Task ExamPost_AnonymousUser_SubmitDemo()
        {
            var request = this.Request(new ExamAttempt { TechnologyId = techA.Id });

            var response = await this.manager.AttemptGet(request);

            var exam = response.Item;

            foreach (var q in exam.Questions)
            {
                foreach (var a in q.AttemptAnswers)
                {
                    a.Value = a.TaskAnswer.Value;
                }
            }

            var submitRequest = this.Request(exam);
            var submitResponse = await this.manager.AttemptPost(submitRequest);

            exam = submitResponse.Item;
            Assert.AreEqual(ResponseCode.Success, submitResponse.Code);
            Assert.NotNull(exam);
            Assert.AreEqual(0, exam.Id);
            Assert.True(exam.IsFinished);
            Assert.True(exam.IsPassed);
            Assert.True(exam.ExamScore == exam.MaxScore);

            int count = this.db.ExamAttempt.Count();
            Assert.AreEqual(0, count);
        }

        [TestCase]
        public async Task ExamPost_Member_SubmitExamAttempt()
        {
            var newAttemptRequest = this.Request(new ExamAttempt { TaskId = this.testB.Id }, this.expertA);

            var newAttemptResponse = await this.manager.AttemptGet(newAttemptRequest);

            Assert.AreEqual(ResponseCode.Success, newAttemptResponse.Code);

            var exam = newAttemptResponse.Item;

            Assert.NotNull(exam);
            Assert.True(this.IsAttemptDataComplete(exam));

            // submit intermediate results
            var request = this.Request(exam, this.expertA);
            var response = await this.manager.AttemptPost(request);

            exam = response.Item;
            Assert.AreEqual(ResponseCode.Success, response.Code);
            Assert.NotNull(exam);
            Assert.True(this.IsAttemptDataComplete(exam));
            Assert.False(exam.IsFinished);
            Assert.Null(exam.IsPassed);

            // submit for evaluation
            Request<ExamAttempt> submitRequest;
            using (var context = NewDbContext())
            {
                IExamManager m = NewManager(context);
                var attempt = await m.AttemptGet(this.Request(new ExamAttempt { Id = exam.Id }, this.expertA));
                attempt.Item.IsFinished = true;
                submitRequest = this.Request(attempt.Item, this.expertA);
            }

            var submitResponse = await this.manager.AttemptPost(submitRequest);

            exam = submitResponse.Item;
            Assert.AreEqual(ResponseCode.Success, submitResponse.Code);
            Assert.True(this.IsAttemptDataComplete(exam));
            Assert.True(exam.IsFinished);
            Assert.NotNull(exam.IsPassed);
            Assert.AreEqual(0, exam.ExamScore);

            // attempt to submit after comletion should not change anything
            using (var context = NewDbContext())
            {
                IExamManager m = NewManager(context);
                var attempt = await m.AttemptGet(this.Request(new ExamAttempt { Id = exam.Id }, this.expertA));
                attempt.Item.IsFinished = true;
                attempt.Item.IsPassed = true;
                attempt.Item.ExamScore = null;
                foreach (var q in attempt.Item.Questions)
                {
                    foreach (var a in q.AttemptAnswers)
                    {
                        a.Value = a.TaskAnswer.Value;
                    }
                }
                submitRequest = this.Request(attempt.Item, this.expertA);
            }

            submitResponse = await this.manager.AttemptPost(submitRequest);

            exam = submitResponse.Item;
            Assert.AreEqual(ResponseCode.Success, submitResponse.Code);
            Assert.True(this.IsAttemptDataComplete(exam));
            Assert.True(exam.IsFinished);
            Assert.NotNull(exam.IsPassed);
            Assert.AreEqual(0, exam.ExamScore);

            this.dbManager.Remove(exam);
            this.dbManager.SaveChanges();
        }

        [TestCase]
        public async Task ExamResult_Member_ActiveAttemptAccess()
        {
            var newAttemptRequest = this.Request(new ExamAttempt { TaskId = this.testB.Id }, this.expertA);

            var newAttemptResponse = await this.manager.AttemptGet(newAttemptRequest);

            var exam = newAttemptResponse.Item;
            Assert.AreEqual(ResponseCode.Success, newAttemptResponse.Code);
            Assert.NotNull(exam);


            // guests cannot see tests results
            var resultRequest = this.Request(exam.Id);
            var resultResponse = await this.manager.Results(resultRequest);

            Assert.AreEqual(ResponseCode.NotFound, resultResponse.Code);
            Assert.Null(resultResponse.Item);


            // non expert cannot see intermediate results
            resultRequest = this.Request(exam.Id, this.expertA);
            resultResponse = await this.manager.Results(resultRequest);

            Assert.AreEqual(ResponseCode.NotFound, resultResponse.Code);
            Assert.Null(resultResponse.Item);


            // expert can monitor active attempts
            resultRequest = this.Request(exam.Id, this.expertB);
            resultResponse = await this.manager.Results(resultRequest);

            exam = resultResponse.Item;
            Assert.AreEqual(ResponseCode.Success, resultResponse.Code);
            Assert.True(this.IsAttemptDataComplete(exam));
            Assert.False(exam.IsFinished);


            this.dbManager.Remove(exam);
            this.dbManager.SaveChanges();
        }

        [TestCase]
        public async Task ExamResult_Member_FinishedAttemptAccess()
        {
            var newAttemptRequest = this.Request(new ExamAttempt { TaskId = this.testB.Id }, this.expertA);

            var newAttemptResponse = await this.manager.AttemptGet(newAttemptRequest);

            var exam = newAttemptResponse.Item;
            Assert.AreEqual(ResponseCode.Success, newAttemptResponse.Code);
            Assert.NotNull(exam);

            exam.IsFinished = true;
            await this.manager.AttemptPost(this.Request(exam, this.expertA));

            // users can see their finished attempts
            var resultRequest = this.Request(exam.Id, this.expertA);
            var resultResponse = await this.manager.Results(resultRequest);

            exam = resultResponse.Item;
            Assert.AreEqual(ResponseCode.Success, resultResponse.Code);
            Assert.True(this.IsAttemptDataComplete(exam));
            Assert.True(exam.IsFinished);

            // non-experts can see finished attempts
            resultRequest = this.Request(exam.Id, this.expertC);
            resultResponse = await this.manager.Results(resultRequest);

            exam = resultResponse.Item;
            Assert.AreEqual(ResponseCode.Success, resultResponse.Code);
            Assert.True(this.IsAttemptDataComplete(exam));
            Assert.True(exam.IsFinished);


            this.dbManager.Remove(exam);
            this.dbManager.SaveChanges();
        }

        [TestCase]
        public async Task ExamDelete_Owner()
        {
            var examRequest = this.Request(new ExamAttempt { TaskId = this.testB.Id }, this.expertA);
            var examResponse = await this.manager.AttemptGet(examRequest);

            var exam = examResponse.Item;
            Assert.AreEqual(ResponseCode.Success, examResponse.Code);

            var delRequest = this.Request(exam.Id, this.expertA);

            var delPreviewResponse = await this.manager.DeletePreview(delRequest);

            Assert.AreEqual(ResponseCode.Success, delPreviewResponse.Code);
            Assert.NotNull(delPreviewResponse.Item);
            Assert.AreEqual(exam.Id, delPreviewResponse.Item.Id);

            int count = this.db.ExamAttempt.Count(a => a.Id == exam.Id);
            Assert.AreEqual(1, count);

            var delResponse = await this.manager.Delete(delRequest);

            Assert.AreEqual(ResponseCode.Success, delResponse.Code);
            Assert.True(delResponse.Item);

            count = this.db.ExamAttempt.Count(a => a.Id == exam.Id);
            Assert.AreEqual(0, count);
        }

        [TestCase]
        public async Task ExamDelete_WrongId()
        {
            var examRequest = this.Request(new ExamAttempt { TaskId = this.testB.Id }, this.expertA);
            var examResponse = await this.manager.AttemptGet(examRequest);

            var exam = examResponse.Item;
            Assert.AreEqual(ResponseCode.Success, examResponse.Code);

            var delRequest = this.Request(314, this.expertA);

            var delPreviewResponse = await this.manager.DeletePreview(delRequest);

            Assert.AreEqual(ResponseCode.NotFound, delPreviewResponse.Code);
            Assert.Null(delPreviewResponse.Item);

            int count = this.db.ExamAttempt.Count();
            Assert.AreEqual(1, count);

            var delResponse = await this.manager.Delete(delRequest);

            Assert.AreEqual(ResponseCode.NotFound, delResponse.Code);

            count = this.db.ExamAttempt.Count();
            Assert.AreEqual(1, count);

            this.db.Remove(exam);
            this.db.SaveChanges();
        }

        [TestCase]
        public async Task ExamDelete_Expert()
        {
            var examRequest = this.Request(new ExamAttempt { TaskId = this.testB.Id }, this.expertA);
            var examResponse = await this.manager.AttemptGet(examRequest);

            var exam = examResponse.Item;
            Assert.AreEqual(ResponseCode.Success, examResponse.Code);

            var delRequest = this.Request(exam.Id, this.expertB);

            // experts cannot delete test results
            var delPreviewResponse = await this.manager.DeletePreview(delRequest);

            Assert.AreEqual(ResponseCode.NotFound, delPreviewResponse.Code);
            Assert.Null(delPreviewResponse.Item);

            int count = this.db.ExamAttempt.Count(a => a.Id == exam.Id);
            Assert.AreEqual(1, count);

            var delResponse = await this.manager.Delete(delRequest);

            Assert.AreEqual(ResponseCode.NotFound, delResponse.Code);

            count = this.db.ExamAttempt.Count(a => a.Id == exam.Id);
            Assert.AreEqual(1, count);

            this.db.Remove(exam);
            this.db.SaveChanges();
        }
        #endregion
    }
}
