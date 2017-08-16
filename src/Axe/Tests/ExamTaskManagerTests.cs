using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Axe.Managers;
using Axe.Models;
using Axe.Models.ExamTasksVm;

namespace Axe.Tests
{
    public class ExamTaskManagerTests : DbDependentTests
    {
        private AxeDbContext dbManager;
        private IExamTaskManager manager;
        private TaskQuestion[] questions;

        #region Setup

        [OneTimeSetUp]
        public void InitTestFixture()
        {
            this.InitStorage("TestsManagerDb");
            this.InitTechnologies();
            this.InitQuestions();
        }

        /// <summary>
        /// Creates questions with answers in each technology
        /// </summary>
        private void InitQuestions()
        {
            questions = new[] { this.techA, this.techB, this.techC }
                        .SelectMany(t => Enumerable.Range(1, 3).Select(i => CreateQuestion(t, "Q" + i)))
                        .ToArray();

            using (var storage = NewDbContext())
            {
                storage.AddRange(questions);
                storage.SaveChanges();
            }
        }

        private TaskQuestion CreateQuestion(Technology t, string text)
        {
            var question = new TaskQuestion
            {
                TechnologyId = t.Id,
                Text = text,
                Type = TaskQuestionType.SingleLine,
            };
            question.Answers = new List<TaskAnswer>
            {
                new TaskAnswer { Score = 1, Value = "+"}
            };
            return question;
        }

        [SetUp]
        public void InitTestCase()
        {
            this.db = NewDbContext();

            this.dbManager = NewDbContext();
            this.manager = new ExamTaskManager(this.dbManager);
        }

        [TearDown]
        public void ClearTestCase()
        {
            this.dbManager.Dispose();
            this.db.Dispose();
        }

        #endregion


        private bool IsTaskInputDataComplete(TaskInputVm test)
        {
            return test.TechnologyId > 0 && String.IsNullOrWhiteSpace(test.TechnologyName) == false &&
                test.Questions.Count > 0 && test.Questions.All(q => q.Score > 0);
        }

        [TestCase]
        public async Task TestGet_NonExpert()
        {
            var request = this.Request(new ExamTask { TechnologyId = this.techA.Id }, this.expertB);

            var response = await this.manager.InputGet(request);

            Assert.AreEqual(ResponseCode.NotFound, response.Code);
            Assert.Null(response.Item);
        }

        [TestCase]
        public async Task TestGet_WrongTechnologyId()
        {
            var request = this.Request(new ExamTask { TechnologyId = 100 }, this.expertB);

            var response = await this.manager.InputGet(request);

            Assert.AreEqual(ResponseCode.NotFound, response.Code);
            Assert.Null(response.Item);
        }

        [TestCase]
        public async Task TestGet_WrongId()
        {
            var request = this.Request(new ExamTask { Id = 100 }, this.expertA);

            var response = await this.manager.InputGet(request);

            Assert.AreEqual(ResponseCode.NotFound, response.Code);
            Assert.Null(response.Item);
        }

        [TestCase]
        public async Task TestGet_Expert()
        {
            var request = this.Request(new ExamTask { TechnologyId = this.techA.Id }, this.expertA);

            var response = await this.manager.InputGet(request);

            Assert.AreEqual(ResponseCode.Success, response.Code);
            var test = response.Item;
            Assert.NotNull(test);
            Assert.True(this.IsTaskInputDataComplete(test));
            Assert.Zero(test.Id);
            Assert.AreEqual(3, test.Questions.Count);
            Assert.False(test.Questions.Any(q => q.IsSelected));
        }

        private int AddExamTask(Technology t)
        {
            var existingTest = new ExamTask
            {
                TechnologyId = t.Id,
            };
            existingTest.Questions = this.questions.Where(q => q.TechnologyId == t.Id)
                                         .Select(q => new TaskQuestionLink { Task = existingTest, QuestionId = q.Id })
                                         .ToList();
            using (var storage = NewDbContext())
            {
                storage.Add(existingTest);
                storage.SaveChanges();
            }

            return existingTest.Id;
        }

        private void DeleteExamTask(int id)
        {
            using (var storage = NewDbContext())
            {
                var test = storage.ExamTask.Include(t => t.Questions).Single(t => t.Id == id);
                storage.Remove(test);
                storage.SaveChanges();
            }
        }

        [TestCase]
        public async Task TestGet_ExistingTestNonExpert()
        {
            int id = this.AddExamTask(this.techC);

            try
            {
                var request = this.Request(new ExamTask { Id = id }, this.expertA);
                var response = await this.manager.InputGet(request);

                Assert.AreEqual(ResponseCode.NotFound, response.Code);
                Assert.Null(response.Item);
            }
            finally
            {
                this.DeleteExamTask(id);
            }
        }

        [TestCase]
        public async Task TestGet_ExistingTestExpert()
        {
            int id = this.AddExamTask(this.techC);

            try
            {
                var request = this.Request(new ExamTask { Id = id, TechnologyId = this.techA.Id }, this.expertC);
                var response = await this.manager.InputGet(request);

                Assert.AreEqual(ResponseCode.Success, response.Code);
                var test = response.Item;
                Assert.NotNull(test);
                Assert.True(this.IsTaskInputDataComplete(test));
                Assert.AreEqual(id, test.Id);
                Assert.AreEqual(this.techC.Id, test.TechnologyId);
                Assert.AreEqual(3, test.Questions.Count);
                Assert.True(test.Questions.All(q => q.IsSelected));
            }
            finally
            {
                this.DeleteExamTask(id);
            }
        }

        [TestCase]
        public async Task TestPost_CreateNonExpert()
        {
            var input = new TaskInputVm
            {
                TechnologyId = this.techA.Id,
                Questions = this.questions.Where(q => q.TechnologyId == this.techA.Id)
                                .Select(q => new QuestionSelectionVm { Id = q.Id, IsSelected = true })
                                .ToList()
            };

            var request = this.Request(input, this.expertB);
            var response = await this.manager.InputPost(request);

            Assert.AreEqual(ResponseCode.ValidationError, response.Code);
            var test = response.Item;
            Assert.True(this.IsTaskInputDataComplete(test));
            Assert.Zero(test.Id);

            int count = this.db.ExamTask.Count();
            Assert.Zero(count);

            Assert.False(request.ModelState.IsValid);
            Assert.AreEqual(1, request.ModelState.ErrorCount);
            Assert.True(request.ModelState[String.Empty].Errors.Any(e => e.ErrorMessage == ValidationMessages.Instance.TaskExpertInput(this.techA.Name)));
        }

        [TestCase]
        public async Task TestPost_CreateExpert()
        {
            var input = new TaskInputVm
            {
                TechnologyId = this.techA.Id,
                Questions = this.questions.Where(q => q.TechnologyId == this.techA.Id)
                                .Select(q => new QuestionSelectionVm { Id = q.Id, IsSelected = true })
                                .ToList()
            };

            int id = -1;
            try
            {
                var request = this.Request(input, this.expertA);
                var response = await this.manager.InputPost(request);

                Assert.AreEqual(ResponseCode.Success, response.Code);
                id = response.Item.Id;

                var test = await this.manager.InputGet(this.Request(new ExamTask { Id = id }, this.expertA));

                Assert.True(this.IsTaskInputDataComplete(test.Item));
            }
            finally
            {
                if (id > 0)
                {
                    this.DeleteExamTask(id);
                }
            }
        }

        [TestCase]
        public async Task TestPost_InvalidInputNoQuestions()
        {
            var input = new TaskInputVm
            {
                TechnologyId = this.techA.Id,
                Questions = this.questions.Where(q => q.TechnologyId == this.techA.Id)
                                .Select(q => new QuestionSelectionVm { Id = q.Id, IsSelected = false })
                                .ToList()
            };

            var request = this.Request(input, this.expertA);
            var response = await this.manager.InputPost(request);

            Assert.AreEqual(ResponseCode.ValidationError, response.Code);
            var test = response.Item;
            Assert.True(this.IsTaskInputDataComplete(test));
            Assert.Zero(test.Id);

            int count = this.db.ExamTask.Count();
            Assert.Zero(count);

            Assert.False(request.ModelState.IsValid);
            Assert.AreEqual(1, request.ModelState.ErrorCount);
            Assert.True(request.ModelState[String.Empty].Errors.Any(e => e.ErrorMessage == ValidationMessages.Instance.TaskNoQuestions));
        }

        [TestCase]
        public async Task TestPost_InvalidInputUnknownTech()
        {
            var input = new TaskInputVm
            {
                TechnologyId = 100,
                Questions = this.questions.Where(q => q.TechnologyId == this.techA.Id)
                                .Select(q => new QuestionSelectionVm { Id = q.Id, IsSelected = true })
                                .ToList()
            };

            var request = this.Request(input, this.expertA);
            var response = await this.manager.InputPost(request);

            Assert.AreEqual(ResponseCode.ValidationError, response.Code);
            var test = response.Item;
            Assert.False(this.IsTaskInputDataComplete(test));
            Assert.True(test.Questions.Count > 0);
            Assert.Zero(test.Id);

            int count = this.db.ExamTask.Count();
            Assert.Zero(count);

            Assert.False(request.ModelState.IsValid);
            Assert.AreEqual(1, request.ModelState.ErrorCount);
            Assert.True(request.ModelState[String.Empty].Errors.Any(e => e.ErrorMessage == ValidationMessages.Instance.UnknownTechnology));
        }

        [TestCase]
        public async Task TestPost_EditNonExpert()
        {
            int id = this.AddExamTask(this.techA);

            var input = new TaskInputVm
            {
                Id = id,
                TechnologyId = this.techA.Id,
                Questions = this.questions.Where(q => q.TechnologyId == this.techA.Id)
                                .Select((q, i) => new QuestionSelectionVm { Id = q.Id, IsSelected = i % 2 == 1 })
                                .ToList()
            };

            try
            {
                var request = this.Request(input, this.expertB);
                var response = await this.manager.InputPost(request);

                Assert.AreEqual(ResponseCode.ValidationError, response.Code);
                var test = response.Item;
                Assert.True(this.IsTaskInputDataComplete(test));

                var exam = this.db.ExamTask.Include(t => t.Questions).Single(t => t.Id == id);
                Assert.AreEqual(3, exam.Questions.Count);

                Assert.False(request.ModelState.IsValid);
                Assert.AreEqual(1, request.ModelState.ErrorCount);
                Assert.True(request.ModelState[String.Empty].Errors.Any(e => e.ErrorMessage == ValidationMessages.Instance.TaskExpertInput(this.techA.Name)));

            }
            finally
            {
                this.DeleteExamTask(id);
            }
        }

        [TestCase]
        public async Task TestPost_EditExpert()
        {
            int id = this.AddExamTask(this.techA);

            var input = new TaskInputVm
            {
                Id = id,
                TechnologyId = this.techA.Id,
                Questions = this.questions.Where(q => q.TechnologyId == this.techA.Id)
                                .Select((q, i) => new QuestionSelectionVm { Id = q.Id, IsSelected = i % 2 == 1 })
                                .ToList()
            };

            try
            {
                var request = this.Request(input, this.expertA);
                var response = await this.manager.InputPost(request);

                Assert.AreEqual(ResponseCode.Success, response.Code);

                var exam = this.db.ExamTask.Include(t => t.Questions).Single(t => t.Id == id);
                Assert.AreEqual(1, exam.Questions.Count);
            }
            finally
            {
                this.DeleteExamTask(id);
            }
        }
        [TestCase]
        public async Task TestDetails_WrongId()
        {
            var request = this.Request((int?)100, this.expertC);

            var response = await this.manager.DetailsGet(request);

            Assert.AreEqual(ResponseCode.NotFound, response.Code);
            Assert.Null(response.Item);
        }

        [TestCase]
        public async Task TestDetails_NonExpert()
        {
            int id = this.AddExamTask(this.techA);

            try
            {
                var request = this.Request((int?)id, this.expertB);

                var response = await this.manager.DetailsGet(request);

                Assert.AreEqual(ResponseCode.NotFound, response.Code);
                Assert.Null(response.Item);
            }
            finally
            {
                this.DeleteExamTask(id);
            }
        }

        [TestCase]
        public async Task TestDetails_Expert()
        {
            int id = this.AddExamTask(this.techA);

            try
            {
                var request = this.Request((int?)id, this.expertA);

                var response = await this.manager.DetailsGet(request);

                Assert.AreEqual(ResponseCode.Success, response.Code);
                var test = response.Item;
                Assert.NotNull(test);
                Assert.True(test.Technology.Id == this.techA.Id);
                Assert.AreEqual(3, test.Questions.Count);
            }
            finally
            {
                this.DeleteExamTask(id);
            }
        }

        [TestCase]
        public async Task TestDelete_WrongId()
        {
            var request = this.Request(100, this.expertC);

            var response = await this.manager.DeleteGet(request);

            Assert.AreEqual(ResponseCode.NotFound, response.Code);
            Assert.Null(response.Item);

            response = await this.manager.DeletePost(request);

            Assert.AreEqual(ResponseCode.NotFound, response.Code);
            Assert.Null(response.Item);
        }

        [TestCase]
        public async Task TestDelete_NonExpert()
        {
            int id = this.AddExamTask(this.techA);

            try
            {
                var request = this.Request(id, this.expertB);

                var response = await this.manager.DeleteGet(request);

                Assert.AreEqual(ResponseCode.NotFound, response.Code);
                Assert.Null(response.Item);


                response = await this.manager.DeletePost(request);

                Assert.AreEqual(ResponseCode.ValidationError, response.Code);
                Assert.AreEqual(0, response.Item.Id);

                int count = this.db.ExamTask.Count(t => t.Id == id);
                Assert.AreEqual(1, count);
            }
            finally
            {
                this.DeleteExamTask(id);
            }
        }

        [TestCase]
        public async Task TestDelete_Expert()
        {
            int id = this.AddExamTask(this.techA);

            var request = this.Request(id, this.expertA);

            var response = await this.manager.DeleteGet(request);

            Assert.AreEqual(ResponseCode.Success, response.Code);
            var test = response.Item;
            Assert.NotNull(test);

            int count = this.db.ExamTask.Count(t => t.Id == id);
            Assert.AreEqual(1, count);


            response = await this.manager.DeletePost(request);

            Assert.AreEqual(ResponseCode.Success, response.Code);
            test = response.Item;

            count = this.db.ExamTask.Count(t => t.Id == id);
            Assert.AreEqual(0, count);
        }
    }
}
