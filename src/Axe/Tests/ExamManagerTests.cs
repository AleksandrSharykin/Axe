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

        [SetUp]
        public void InitTestCase()
        {
            this.db = NewDbContext();

            this.dbManager = NewDbContext();
            this.manager = new ExamManager(this.dbManager, new ExamEvaluator());
        }

        [TearDown]
        public void ClearTestCase()
        {
            this.dbManager.Dispose();
            this.db.Dispose();
        }

        [TestCase]
        public async Task ExamGet_AnonymousUser_TechnologyDemo()
        {
            var request = this.Request(new ExamAttempt { TechnologyId = techA.Id });

            var response = await this.manager.AttemptGet(request);

            Assert.AreEqual(ResponseCode.Success, response.Code);
            Assert.AreEqual(this.demoA.Id, response.Item.Task.Id);
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
        }

        [TestCase]
        public async Task ExamGet_AnonymousUser_TaskDemoNoAccess()
        {
            var request = this.Request(new ExamAttempt { TaskId = this.testA.Id });

            var response = await this.manager.AttemptGet(request);

            Assert.AreEqual(ResponseCode.NotFound, response.Code);
            Assert.Null(response.Item);
        }
        #endregion
    }
}
