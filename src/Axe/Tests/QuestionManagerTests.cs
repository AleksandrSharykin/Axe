using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Axe.Managers;
using Axe.Models;
using Axe.Models.QuestionsVm;

namespace Axe.Tests
{
    public class QuestionManagerTests : DbDependentTests
    {
        private AxeDbContext dbManager;
        private IQuestionManager manager;

        #region Setup
        [OneTimeSetUp]
        public void InitTestFixture()
        {
            this.InitStorage("QuestionManagerDb");
            this.InitTechnologies();
        }

        [SetUp]
        public void InitTestCase()
        {
            this.db = NewDbContext();

            this.dbManager = NewDbContext();
            this.manager = new QuestionManager(this.dbManager);
        }

        [TearDown]
        public void ClearTestCase()
        {
            this.dbManager.Dispose();
            this.db.Dispose();
        }
        #endregion

        private bool IsQuestionDataComplete(QuestionInputVm q)
        {
            return
                q.Technologies != null && q.Technologies.Count() > 0 &&
                q.Answers != null && q.Answers.Count > 0;
        }

        [TestCase]
        public async Task QuestionGet_CreateUnknownTech()
        {
            var request = this.Request(new TaskQuestion { TechnologyId = 100 }, this.expertA);

            var response = await this.manager.InputGet(request);

            Assert.AreEqual(ResponseCode.NotFound, response.Code);
            Assert.Null(response.Item);
        }

        [TestCase]
        public async Task QuestionGet_CreateNonExpert()
        {
            var request = this.Request(new TaskQuestion { TechnologyId = this.techA.Id }, this.expertB);

            var response = await this.manager.InputGet(request);

            Assert.AreEqual(ResponseCode.NotFound, response.Code);
            Assert.Null(response.Item);
        }

        [TestCase]
        public async Task QuestionGet_CreateExpert()
        {
            var request = this.Request(new TaskQuestion { TechnologyId = this.techA.Id }, this.expertA);

            var response = await this.manager.InputGet(request);

            Assert.AreEqual(ResponseCode.Success, response.Code);

            var question = response.Item;
            Assert.NotNull(question);
            Assert.True(this.IsQuestionDataComplete(question));
            Assert.AreEqual(this.techA.Id, question.TechnologyId);
            Assert.True(question.Technologies.Count() == 1 && question.Technologies.Any(t => t.Value == this.techA.Id.ToString()));

            Assert.AreEqual(TaskQuestionType.MultiChoice, question.EditorType);
            Assert.AreEqual(4, question.Answers.Count);
            Assert.False(question.WithUserInput);
        }

        [TestCase]
        public async Task QuestionGet_CreateWrongId()
        {
            var request = this.Request(new TaskQuestion { Id = 100, TechnologyId = this.techA.Id }, this.expertA);

            var response = await this.manager.InputGet(request);

            Assert.AreEqual(ResponseCode.Success, response.Code);

            var question = response.Item;
            Assert.NotNull(question);
            Assert.True(this.IsQuestionDataComplete(question));
        }

        [TestCase]
        public async Task QuestionGet_CreateSingleChoice()
        {
            var request = this.Request(new TaskQuestion { TechnologyId = this.techB.Id, Type = TaskQuestionType.SingleChoice }, this.expertB);

            var response = await this.manager.InputGet(request);

            Assert.AreEqual(ResponseCode.Success, response.Code);

            var question = response.Item;
            Assert.NotNull(question);
            Assert.True(this.IsQuestionDataComplete(question));
            Assert.AreEqual(this.techB.Id, question.TechnologyId);
            Assert.True(question.Technologies.Count() == 1 && question.Technologies.Any(t => t.Value == this.techB.Id.ToString()));

            Assert.AreEqual(TaskQuestionType.SingleChoice, question.EditorType);
            Assert.AreEqual(4, question.Answers.Count);
            Assert.False(question.WithUserInput);
        }

        [TestCase]
        public async Task QuestionGet_CreateMultiLine()
        {
            var request = this.Request(new TaskQuestion { TechnologyId = this.techA.Id, Type = TaskQuestionType.MultiLine }, this.expertA);

            var response = await this.manager.InputGet(request);

            Assert.AreEqual(ResponseCode.Success, response.Code);

            var question = response.Item;
            Assert.NotNull(question);
            Assert.True(this.IsQuestionDataComplete(question));
            Assert.AreEqual(this.techA.Id, question.TechnologyId);
            Assert.True(question.Technologies.Count() == 1 && question.Technologies.Any(t => t.Value == this.techA.Id.ToString()));

            Assert.AreEqual(TaskQuestionType.MultiLine, question.EditorType);
            Assert.AreEqual(1, question.Answers.Count);
            Assert.True(question.WithUserInput);
        }

        [TestCase]
        public async Task QuestionGet_CreateSingleLine()
        {
            var request = this.Request(new TaskQuestion { TechnologyId = this.techA.Id, Type = TaskQuestionType.SingleLine }, this.expertA);

            var response = await this.manager.InputGet(request);

            Assert.AreEqual(ResponseCode.Success, response.Code);

            var question = response.Item;
            Assert.NotNull(question);
            Assert.True(this.IsQuestionDataComplete(question));
            Assert.AreEqual(this.techA.Id, question.TechnologyId);
            Assert.True(question.Technologies.Count() == 1 && question.Technologies.Any(t => t.Value == this.techA.Id.ToString()));

            Assert.AreEqual(TaskQuestionType.SingleLine, question.EditorType);
            Assert.AreEqual(1, question.Answers.Count);
            Assert.True(question.WithUserInput);
        }


        [TestCase]
        public async Task QuestionGet_EditNonExpert()
        {
            var question = new TaskQuestion
            {
                TechnologyId = this.techA.Id,
                Type = TaskQuestionType.SingleLine,
            };
            question.Answers = new List<TaskAnswer>
            {
                new TaskAnswer { Question = question, },
            };

            this.db.Add(question);
            this.db.SaveChanges();

            try
            {
                var request = this.Request(new TaskQuestion { Id = question.Id }, this.expertB);
                var response = await this.manager.InputGet(request);

                Assert.AreEqual(ResponseCode.NotFound, response.Code);
                Assert.Null(response.Item);
            }
            finally
            {
                this.db.Remove(question);
                this.db.SaveChanges();
            }
        }

        [TestCase]
        public async Task QuestionGet_EditExpert()
        {
            var question = new TaskQuestion
            {
                TechnologyId = this.techA.Id,
                Type = TaskQuestionType.SingleChoice,
            };
            question.Answers = new List<TaskAnswer>
            {
                new TaskAnswer { Question = question, },
                new TaskAnswer { Question = question, },
            };

            this.db.Add(question);
            this.db.SaveChanges();

            try
            {
                var request = this.Request(new TaskQuestion { Id = question.Id, TechnologyId = this.techB.Id, Type = TaskQuestionType.SingleLine }, this.expertA);
                var response = await this.manager.InputGet(request);

                Assert.AreEqual(ResponseCode.Success, response.Code);
                var q = response.Item;
                Assert.NotNull(q);
                Assert.True(this.IsQuestionDataComplete(q));
                Assert.AreEqual(this.techA.Id, q.TechnologyId);
                Assert.AreEqual(TaskQuestionType.SingleChoice, q.EditorType);
                Assert.AreEqual(2, q.Answers.Count);
                Assert.False(q.WithUserInput);
            }
            finally
            {
                this.db.Remove(question);
                this.db.SaveChanges();
            }
        }

        [TestCase]
        public async Task QuestionPost_CreateNonExpert()
        {
            var questionInput = new QuestionInputVm
            {
                TechnologyId = this.techA.Id,
                EditorType = TaskQuestionType.MultiChoice,
                Text = "question",
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { Text = "A1", Score = 1, Value = Boolean.TrueString },
                    new TaskAnswer { Text = "A2", Score = 0, Value = Boolean.FalseString },
                    new TaskAnswer { Text = "A3", Score = 1, Value = Boolean.TrueString },
                }
            };

            var request = this.Request(questionInput, this.expertB);
            var response = await this.manager.InputPost(request);

            Assert.AreEqual(ResponseCode.ValidationError, response.Code);
            var question = response.Item;
            Assert.NotNull(question);
            Assert.True(this.IsQuestionDataComplete(question));

            Assert.False(request.ModelState.IsValid);
            Assert.AreEqual(1, request.ModelState.ErrorCount);
            Assert.True(request.ModelState[String.Empty].Errors.Any(e => e.ErrorMessage == ValidationMessages.Instance.QuestionExpertInput(this.techA.Name)));
        }

        [TestCase]
        public async Task QuestionPost_CreateMultiChoice()
        {
            string text = "it is a question";
            var questionInput = new QuestionInputVm
            {
                TechnologyId = this.techA.Id,
                EditorType = TaskQuestionType.MultiChoice,
                Text = text,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { Text = "A1", Score = 1, Value = Boolean.TrueString },
                    new TaskAnswer { Text = "A2", Score = 0, Value = Boolean.FalseString },
                    new TaskAnswer { Text = "A3", Score = 1, Value = Boolean.TrueString },
                }
            };

            var request = this.Request(questionInput, this.expertA);
            var response = await this.manager.InputPost(request);

            Assert.AreEqual(ResponseCode.Success, response.Code);
            var question = this.db.TaskQuestion.Include(q => q.Answers).Single(q => q.Id == response.Item.Id);

            Assert.AreEqual(this.techA.Id, question.TechnologyId);
            Assert.AreEqual(TaskQuestionType.MultiChoice, question.Type);
            Assert.AreEqual(text, question.Text);
            Assert.AreEqual(3, question.Answers.Count);
            Assert.True(question.Answers.All(a => questionInput.Answers.Any(x => x.Id == a.Id && x.Text == a.Text && x.Score == a.Score && x.Value == a.Value)));

            var item = this.db.TaskQuestion.Include(q => q.Answers).Single();
            this.db.Remove(item);
            this.db.SaveChanges();
        }

        [TestCase]
        public async Task QuestionPost_CreateSingleChoice()
        {
            string text = "it is a question";
            var questionInput = new QuestionInputVm
            {
                TechnologyId = this.techA.Id,
                EditorType = TaskQuestionType.SingleChoice,
                Text = text,
                SelectedAnswer = 0,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { Text = "A1", Score = 1, Value = Boolean.TrueString },
                    new TaskAnswer { Text = "A2", Score = 0, Value = Boolean.FalseString },
                    new TaskAnswer { Text = "A3", Score = 0, Value = Boolean.FalseString },
                }
            };

            var request = this.Request(questionInput, this.expertA);
            var response = await this.manager.InputPost(request);

            Assert.AreEqual(ResponseCode.Success, response.Code);
            var question = this.db.TaskQuestion.Include(q => q.Answers).Single(q => q.Id == response.Item.Id);

            Assert.AreEqual(this.techA.Id, question.TechnologyId);
            Assert.AreEqual(TaskQuestionType.SingleChoice, question.Type);
            Assert.AreEqual(text, question.Text);
            Assert.AreEqual(3, question.Answers.Count);
            Assert.True(question.Answers.All(a => questionInput.Answers.Any(x => x.Id == a.Id && x.Text == a.Text && x.Score == a.Score && x.Value == a.Value)));

            var item = this.db.TaskQuestion.Include(q => q.Answers).Single();
            this.db.Remove(item);
            this.db.SaveChanges();
        }

        [TestCase]
        public async Task QuestionPost_InvalidMultiChoiceInput_OneAnswer()
        {
            string text = "it is a question";
            var questionInput = new QuestionInputVm
            {
                TechnologyId = this.techA.Id,
                EditorType = TaskQuestionType.MultiChoice,
                Text = text,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { Text = "A1", Score = 1, Value = Boolean.TrueString },
                }
            };

            var request = this.Request(questionInput, this.expertA);
            var response = await this.manager.InputPost(request);

            Assert.AreEqual(ResponseCode.ValidationError, response.Code);
            Assert.NotNull(response.Item);

            Assert.False(request.ModelState.IsValid);
            Assert.AreEqual(1, request.ModelState.ErrorCount);
            Assert.True(request.ModelState[String.Empty].Errors.Any(e => e.ErrorMessage == ValidationMessages.Instance.QuestionTwoChoiceOptions));
        }

        [TestCase]
        public async Task QuestionPost_InvalidMultiChoiceInput_NoScore()
        {
            string text = "it is a question";
            var questionInput = new QuestionInputVm
            {
                TechnologyId = this.techA.Id,
                EditorType = TaskQuestionType.MultiChoice,
                Text = text,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { Text = "A1", Value = Boolean.TrueString },
                    new TaskAnswer { Text = "A2", Value = Boolean.TrueString },
                    new TaskAnswer { Text = "A3", Value = Boolean.FalseString },
                }
            };

            var request = this.Request(questionInput, this.expertA);
            var response = await this.manager.InputPost(request);

            Assert.AreEqual(ResponseCode.ValidationError, response.Code);
            Assert.NotNull(response.Item);

            Assert.False(request.ModelState.IsValid);
            Assert.AreEqual(1, request.ModelState.ErrorCount);
            Assert.True(request.ModelState[String.Empty].Errors.Any(e => e.ErrorMessage == ValidationMessages.Instance.QuestionNeedScorePoints));
        }

        [TestCase]
        public async Task QuestionPost_InvalidSingleChoiceInput_OneAnswer()
        {
            string text = "it is a question";
            var questionInput = new QuestionInputVm
            {
                TechnologyId = this.techA.Id,
                EditorType = TaskQuestionType.SingleChoice,
                Text = text,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { Text = "A1", Score = 1, Value = Boolean.TrueString },
                }
            };

            var request = this.Request(questionInput, this.expertA);
            var response = await this.manager.InputPost(request);

            Assert.AreEqual(ResponseCode.ValidationError, response.Code);
            Assert.NotNull(response.Item);

            Assert.False(request.ModelState.IsValid);
            Assert.AreEqual(1, request.ModelState.ErrorCount);
            Assert.True(request.ModelState[String.Empty].Errors.Any(e => e.ErrorMessage == ValidationMessages.Instance.QuestionTwoChoiceOptions));
        }

        [TestCase]
        public async Task QuestionPost_InvalidSingleChoiceInput_NoSelectedAnswer()
        {
            string text = "it is a question";
            var questionInput = new QuestionInputVm
            {
                TechnologyId = this.techA.Id,
                EditorType = TaskQuestionType.SingleChoice,
                Text = text,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { Text = "A1", Score = 1, Value = Boolean.TrueString },
                    new TaskAnswer { Text = "A1", Score = 1, Value = Boolean.FalseString },
                }
            };

            var request = this.Request(questionInput, this.expertA);
            var response = await this.manager.InputPost(request);

            Assert.AreEqual(ResponseCode.ValidationError, response.Code);
            Assert.NotNull(response.Item);

            Assert.False(request.ModelState.IsValid);
            Assert.AreEqual(1, request.ModelState.ErrorCount);
            Assert.True(request.ModelState[String.Empty].Errors.Any(e => e.ErrorMessage == ValidationMessages.Instance.QuestionNeedAnswer));
        }

        [TestCase]
        public async Task QuestionPost_InvalidSingleLineInput_NoAnswer()
        {
            string text = "it is a question";
            var questionInput = new QuestionInputVm
            {
                TechnologyId = this.techA.Id,
                EditorType = TaskQuestionType.SingleLine,
                Text = text,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { Text = "\t", Score = 1, Value = Boolean.TrueString },
                }
            };

            var request = this.Request(questionInput, this.expertA);
            var response = await this.manager.InputPost(request);

            Assert.AreEqual(ResponseCode.ValidationError, response.Code);
            Assert.NotNull(response.Item);

            Assert.False(request.ModelState.IsValid);
            Assert.AreEqual(1, request.ModelState.ErrorCount);
            Assert.True(request.ModelState[String.Empty].Errors.Any(e => e.ErrorMessage == ValidationMessages.Instance.QuestionNeedAnswer));
        }

        [TestCase]
        public async Task QuestionPost_InvalidMultiLineInput_NoAnswer()
        {
            string text = "it is a question";
            var questionInput = new QuestionInputVm
            {
                TechnologyId = this.techA.Id,
                EditorType = TaskQuestionType.MultiLine,
                Text = text,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { Text = "\t", Score = 1, Value = Boolean.TrueString },
                }
            };

            var request = this.Request(questionInput, this.expertA);
            var response = await this.manager.InputPost(request);

            Assert.AreEqual(ResponseCode.ValidationError, response.Code);
            Assert.NotNull(response.Item);

            Assert.False(request.ModelState.IsValid);
            Assert.AreEqual(1, request.ModelState.ErrorCount);
            Assert.True(request.ModelState[String.Empty].Errors.Any(e => e.ErrorMessage == ValidationMessages.Instance.QuestionNeedAnswer));
        }

        [TestCase]
        public async Task QuestionPost_InvalidMultiLineInput_NoScore()
        {
            string text = "it is a question";
            var questionInput = new QuestionInputVm
            {
                TechnologyId = this.techA.Id,
                EditorType = TaskQuestionType.MultiLine,
                Text = text,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { Text = "123", Value = Boolean.TrueString },
                }
            };

            var request = this.Request(questionInput, this.expertA);
            var response = await this.manager.InputPost(request);

            Assert.AreEqual(ResponseCode.ValidationError, response.Code);
            Assert.NotNull(response.Item);

            Assert.False(request.ModelState.IsValid);
            Assert.AreEqual(1, request.ModelState.ErrorCount);
            Assert.True(request.ModelState[String.Empty].Errors.Any(e => e.ErrorMessage == ValidationMessages.Instance.QuestionNeedScorePoints));
        }

        [TestCase]
        public async Task QuestionPost_EditNonExpert()
        {
            string text = "it is a question";
            var question = new TaskQuestion
            {
                TechnologyId = this.techA.Id,
                Type = TaskQuestionType.MultiChoice,
                Text = text,
            };
            question.Answers = new List<TaskAnswer>
            {
                new TaskAnswer { Question = question, Text = "A1", Score = 1, Value = Boolean.TrueString },
                new TaskAnswer { Question = question, Text = "A2", Score = 0, Value = Boolean.FalseString },
                new TaskAnswer { Question = question, Text = "A3", Score = 1, Value = Boolean.TrueString },
            };

            using (var storage = NewDbContext())
            {
                storage.Add(question);
                storage.SaveChanges();
            }

            var input = new QuestionInputVm
            {
                Id = question.Id,
                TechnologyId = this.techA.Id,
                Text = "new question",
                SelectedAnswer = 0,
                EditorType = TaskQuestionType.SingleChoice,
                Answers = question.Answers.Take(2)
                                .Select(a => new TaskAnswer { Id = a.Id, Text = a.Text + "!", Value = a.Value, Score = a.Score, QuestionId = question.Id })
                                .ToList()
            };

            try
            {
                var request = this.Request(input, this.expertB);

                var response = await this.manager.InputPost(request);

                Assert.AreEqual(ResponseCode.ValidationError, response.Code);

                var item = this.db.TaskQuestion.Include(q => q.Answers).Single(q => q.Id == question.Id);

                Assert.AreEqual(question.Text, item.Text);
                Assert.AreEqual(question.Type, item.Type);
                Assert.AreEqual(this.techA.Id, item.TechnologyId);
                Assert.AreEqual(3, item.Answers.Count);
            }
            finally
            {
                var item = this.db.TaskQuestion.Include(q => q.Answers).Single(q => q.Id == question.Id);
                this.db.Remove(item);
                this.db.SaveChanges();
            }
        }

        [TestCase]
        public async Task QuestionPost_EditExpert()
        {
            string text = "it is a question";
            var question = new TaskQuestion
            {
                TechnologyId = this.techA.Id,
                Type = TaskQuestionType.MultiChoice,
                Text = text,
            };
            question.Answers = new List<TaskAnswer>
            {
                new TaskAnswer { Question = question, Text = "A1", Score = 1, Value = Boolean.TrueString },
                new TaskAnswer { Question = question, Text = "A2", Score = 0, Value = Boolean.FalseString },
                new TaskAnswer { Question = question, Text = "A3", Score = 1, Value = Boolean.TrueString },
            };

            using (var storage = NewDbContext())
            {
                storage.Add(question);
                storage.SaveChanges();
            }

            var input = new QuestionInputVm
            {
                Id = question.Id,
                TechnologyId = this.techA.Id,
                Text = "new question",
                SelectedAnswer = 0,
                EditorType = TaskQuestionType.SingleChoice,
                Answers = question.Answers.Take(2)
                                .Select(a => new TaskAnswer { Id = a.Id, Text = a.Text + "!", Value = a.Value, Score = a.Score, QuestionId = question.Id })
                                .ToList()
            };

            try
            {
                var request = this.Request(input, this.expertA);

                var response = await this.manager.InputPost(request);

                Assert.AreEqual(ResponseCode.Success, response.Code);

                var item = this.db.TaskQuestion.Include(q => q.Answers).Single(q => q.Id == question.Id);

                Assert.AreEqual(input.Text, item.Text);
                Assert.AreEqual(input.EditorType, item.Type);
                Assert.AreEqual(this.techA.Id, item.TechnologyId);
                Assert.AreEqual(2, item.Answers.Count);
            }
            finally
            {
                var item = this.db.TaskQuestion.Include(q => q.Answers).Single(q => q.Id == question.Id);
                this.db.Remove(item);
                this.db.SaveChanges();
            }
        }

        private int AddSingleLineQuestion()
        {
            var question = new TaskQuestion
            {
                TechnologyId = this.techA.Id,
                Type = TaskQuestionType.SingleLine,
                Text = "it is a question",
            };
            question.Answers = new List<TaskAnswer>
            {
                new TaskAnswer { Question = question, Text = "it is an answer", Score = 1, Value = Boolean.TrueString },
            };

            using (var storage = NewDbContext())
            {
                storage.Add(question);
                storage.SaveChanges();
            }

            return question.Id;
        }

        private void RemoveQuestion(int id)
        {
            using (var storage = NewDbContext())
            {
                var question = storage.TaskQuestion.Single(q => q.Id == id);
                storage.Remove(question);
                storage.SaveChanges();
            }
        }

        [TestCase]
        public async Task QuestionDetails_WrongId()
        {
            var request = this.Request(100, this.expertC);

            var response = await this.manager.DetailsGet(request);

            Assert.AreEqual(ResponseCode.NotFound, response.Code);
            Assert.Null(response.Item);
        }

        [TestCase]
        public async Task QuestionDetails_NonExpert()
        {
            int id = this.AddSingleLineQuestion();

            try
            {
                var request = this.Request(id, this.expertB);

                var response = await this.manager.DetailsGet(request);

                Assert.AreEqual(ResponseCode.NotFound, response.Code);
                Assert.Null(response.Item);
            }
            finally
            {
                this.RemoveQuestion(id);
            }
        }

        [TestCase]
        public async Task QuestionDetails_Expert()
        {
            int id = this.AddSingleLineQuestion();

            try
            {
                var request = this.Request(id, this.expertA);

                var response = await this.manager.DetailsGet(request);

                Assert.AreEqual(ResponseCode.Success, response.Code);
                var question = response.Item;
                Assert.NotNull(question);
                Assert.True(question.Technology.Id == this.techA.Id);
                Assert.AreEqual(1, question.Answers.Count);
            }
            finally
            {
                this.RemoveQuestion(id);
            }
        }

        [TestCase]
        public async Task QuestionDelete_WrongId()
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
        public async Task QuestionDelete_NonExpert()
        {
            int id = this.AddSingleLineQuestion();

            try
            {
                var request = this.Request(id, this.expertB);

                var response = await this.manager.DeleteGet(request);

                Assert.AreEqual(ResponseCode.NotFound, response.Code);
                Assert.Null(response.Item);


                response = await this.manager.DeletePost(request);

                Assert.AreEqual(ResponseCode.ValidationError, response.Code);
                Assert.AreEqual(0, response.Item.Id);

                int count = this.db.TaskQuestion.Count(q => q.Id == id);
                Assert.AreEqual(1, count);
            }
            finally
            {
                this.RemoveQuestion(id);
            }
        }

        [TestCase]
        public async Task QuestionDelete_Expert()
        {
            int id = this.AddSingleLineQuestion();

            var request = this.Request(id, this.expertA);

            var response = await this.manager.DeleteGet(request);

            Assert.AreEqual(ResponseCode.Success, response.Code);
            var question = response.Item;
            Assert.NotNull(question);

            int count = this.db.TaskQuestion.Count(q => q.Id == id);
            Assert.AreEqual(1, count);


            response = await this.manager.DeletePost(request);

            Assert.AreEqual(ResponseCode.Success, response.Code);
            question = response.Item;

            count = this.db.TaskQuestion.Count(q => q.Id == id);
            Assert.AreEqual(0, count);

        }

        [TestCase]
        public void QuestionInput_AddAnswer()
        {
            var input = new QuestionInputVm
            {
                Text = "123",
                TechnologyId = techC.Id,
                EditorType = TaskQuestionType.SingleChoice,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { },
                    new TaskAnswer { },
                },
            };

            var request = this.Request(input, this.expertC);

            var response = this.manager.ChangeAnswers(request, true);

            Assert.AreEqual(ResponseCode.Success, response.Code);
            Assert.AreEqual(3, response.Item.Answers.Count);
            Assert.True(this.IsQuestionDataComplete(response.Item));
        }

        [TestCase]
        public void QuestionInput_AddInputAnswerWithNullCollection()
        {
            var input = new QuestionInputVm
            {
                Text = "123",
                TechnologyId = techC.Id,
                EditorType = TaskQuestionType.SingleLine,
            };

            var request = this.Request(input, this.expertC);

            var response = this.manager.ChangeAnswers(request, true);

            Assert.AreEqual(ResponseCode.Success, response.Code);
            Assert.AreEqual(1, response.Item.Answers.Count);
            Assert.True(this.IsQuestionDataComplete(response.Item));
        }

        [TestCase]
        public void QuestionInput_AddChoiceAnswerWithNullCollection()
        {
            var input = new QuestionInputVm
            {
                Text = "123",
                TechnologyId = techC.Id,
                EditorType = TaskQuestionType.SingleChoice,
            };

            var request = this.Request(input, this.expertC);

            var response = this.manager.ChangeAnswers(request, true);

            Assert.AreEqual(ResponseCode.Success, response.Code);
            Assert.AreEqual(2, response.Item.Answers.Count);
            Assert.True(this.IsQuestionDataComplete(response.Item));
        }

        [TestCase]
        public void QuestionInput_RemoveChoiceAnswer()
        {
            var input = new QuestionInputVm
            {
                Text = "123",
                TechnologyId = techC.Id,
                EditorType = TaskQuestionType.SingleChoice,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { },
                    new TaskAnswer { },
                    new TaskAnswer { },
                }
            };

            var request = this.Request(input, this.expertC);

            var response = this.manager.ChangeAnswers(request, false);

            Assert.AreEqual(ResponseCode.Success, response.Code);
            Assert.AreEqual(2, response.Item.Answers.Count);
            Assert.True(this.IsQuestionDataComplete(response.Item));
        }

        [TestCase]
        public void QuestionInput_RemoveChoiceLastAnswer()
        {
            var input = new QuestionInputVm
            {
                Text = "123",
                TechnologyId = techC.Id,
                EditorType = TaskQuestionType.SingleChoice,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { },
                }
            };

            var request = this.Request(input, this.expertC);

            var response = this.manager.ChangeAnswers(request, false);

            Assert.AreEqual(ResponseCode.Success, response.Code);
            Assert.AreEqual(1, response.Item.Answers.Count);
            Assert.True(this.IsQuestionDataComplete(response.Item));
        }

        [TestCase]
        public void QuestionInput_RemoveInputAnswer()
        {
            var input = new QuestionInputVm
            {
                Text = "123",
                TechnologyId = techC.Id,
                EditorType = TaskQuestionType.SingleLine,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { },
                }
            };

            var request = this.Request(input, this.expertC);

            var response = this.manager.ChangeAnswers(request, false);

            Assert.AreEqual(ResponseCode.Success, response.Code);
            Assert.AreEqual(1, response.Item.Answers.Count);
            Assert.True(this.IsQuestionDataComplete(response.Item));
        }

        [TestCase]
        public void QuestionInput_ChangeInputToChoice()
        {
            var input = new QuestionInputVm
            {
                Text = "123",
                TechnologyId = techC.Id,
                EditorType = TaskQuestionType.SingleLine,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { },
                }
            };

            var request = this.Request(input, this.expertC);

            var response = this.manager.ChangeQuestionType(request, TaskQuestionType.MultiChoice);

            Assert.AreEqual(ResponseCode.Success, response.Code);
            Assert.AreEqual(1, response.Item.Answers.Count);
            Assert.False(response.Item.WithUserInput);
            Assert.True(this.IsQuestionDataComplete(response.Item));
        }

        [TestCase]
        public void QuestionInput_ChangeChoiceToInput()
        {
            var input = new QuestionInputVm
            {
                Text = "123",
                TechnologyId = techC.Id,
                EditorType = TaskQuestionType.MultiChoice,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { },
                    new TaskAnswer { },
                    new TaskAnswer { },
                }
            };

            var request = this.Request(input, this.expertC);

            var response = this.manager.ChangeQuestionType(request, TaskQuestionType.MultiLine);

            Assert.AreEqual(ResponseCode.Success, response.Code);
            Assert.AreEqual(1, response.Item.Answers.Count);
            Assert.True(response.Item.WithUserInput);
            Assert.True(this.IsQuestionDataComplete(response.Item));
        }
    }
}
