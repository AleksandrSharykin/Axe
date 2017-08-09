using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Axe.Models;
using Axe.Managers;

namespace Axe.Tests
{
    [TestFixture]
    public class ExamEvaluatorTests
    {
        private readonly string True = Boolean.TrueString;
        private readonly string False = Boolean.FalseString;

        private ExamEvaluator evaluator;
        private TaskQuestion qMultiChoice, qSingleChoice, qMultiLine, qSingleLine;

        [OneTimeSetUp]
        public void InitTestFixture()
        {
            this.evaluator = new ExamEvaluator();

            this.qMultiChoice = new TaskQuestion
            {
                Type = TaskQuestionType.MultiChoice,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { Text = "1", Value = this.False, Score = 0 },
                    new TaskAnswer { Text = "2", Value = this.True, Score = 10 },
                    new TaskAnswer { Text = "3", Value = this.True, Score = 20 },
                    new TaskAnswer { Text = "4", Value = this.False, Score = 0 },
                }
            };

            this.qSingleChoice = new TaskQuestion
            {
                Type = TaskQuestionType.SingleChoice,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { Text = "1", Value = this.False, Score = 0 },
                    new TaskAnswer { Text = "2", Value = this.True, Score = 10 },
                    new TaskAnswer { Text = "3", Value = this.False, Score = 0 },
                    new TaskAnswer { Text = "4", Value = this.False, Score = 0 },
                }
            };

            string multiline = "abc" + Environment.NewLine + "123";
            this.qMultiLine = new TaskQuestion
            {
                Type = TaskQuestionType.MultiLine,
                Answers = new List<TaskAnswer> { new TaskAnswer { Text = multiline, Value = multiline, Score = 40 } }
            };

            string singleline = "100500";
            this.qSingleLine = new TaskQuestion
            {
                Type = TaskQuestionType.SingleLine,
                Answers = new List<TaskAnswer> { new TaskAnswer { Text = singleline, Value = singleline, Score = 10 } }
            };
        }

        [SetUp]
        public void InitTestCase()
        {            
        }

        /// <summary>
        /// Creates exam attempt with provided answers for a task consisting of a single question
        /// </summary>
        /// <param name="question"></param>
        /// <param name="answers"></param>
        /// <returns></returns>
        private ExamAttempt MakeSingleQuestionAttempt(TaskQuestion question, params AttemptAnswer[] answers)
        {
            return new ExamAttempt
            {
                Questions = new List<AttemptQuestion>
                {
                    new AttemptQuestion
                    {
                        TaskQuestion = question,
                        AttemptAnswers = answers,
                    }
                }
            };
        }

        [TestCase]
        public void EvalMultichoiceQuestion_AllAnswersCorrect()
        {
            var A = this.qMultiChoice.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = this.False },
                new AttemptAnswer { TaskAnswer = A[1], Value = this.True },
                new AttemptAnswer { TaskAnswer = A[2], Value = this.True },
                new AttemptAnswer { TaskAnswer = A[3], Value = this.False },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qMultiChoice, answers);

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(true, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(true, attempt.IsPassed);
            Assert.AreEqual(30, attempt.MaxScore);
            Assert.AreEqual(30, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultichoiceQuestion_NoAnswers()
        {
            var A = this.qMultiChoice.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = this.False },
                new AttemptAnswer { TaskAnswer = A[1], Value = this.False },
                new AttemptAnswer { TaskAnswer = A[2], Value = this.False },
                new AttemptAnswer { TaskAnswer = A[3], Value = this.False },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qMultiChoice, answers);

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(false, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(false, attempt.IsPassed);
            Assert.AreEqual(30, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultichoiceQuestion_AllCorrectAnswersAndIncorrect()
        {
            var A = this.qMultiChoice.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = this.False },
                new AttemptAnswer { TaskAnswer = A[1], Value = this.True },
                new AttemptAnswer { TaskAnswer = A[2], Value = this.True },
                new AttemptAnswer { TaskAnswer = A[3], Value = this.True },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qMultiChoice, answers);

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(false, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(false, attempt.IsPassed);
            Assert.AreEqual(30, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultichoiceQuestion_PartiallyCorrectPassed()
        {
            var A = this.qMultiChoice.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = this.False },
                new AttemptAnswer { TaskAnswer = A[1], Value = this.False },
                new AttemptAnswer { TaskAnswer = A[2], Value = this.True },
                new AttemptAnswer { TaskAnswer = A[3], Value = this.False },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qMultiChoice, answers);

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(true, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(true, attempt.IsPassed);
            Assert.AreEqual(30, attempt.MaxScore);
            Assert.AreEqual(20, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultichoiceQuestion_PartiallyCorrectFailed()
        {
            var A = this.qMultiChoice.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = this.False },
                new AttemptAnswer { TaskAnswer = A[1], Value = this.True },
                new AttemptAnswer { TaskAnswer = A[2], Value = this.False },
                new AttemptAnswer { TaskAnswer = A[3], Value = this.False },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qMultiChoice, answers);

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(true, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(false, attempt.IsPassed);
            Assert.AreEqual(30, attempt.MaxScore);
            Assert.AreEqual(10, attempt.ExamScore);
        }
    }
}
