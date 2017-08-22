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
        public static readonly string True = Boolean.TrueString;
        public static readonly string False = Boolean.FalseString;

        public static readonly string None = null;

        /// <summary>
        /// Shortcut for line break sequence
        /// </summary>
        private readonly string NL = Environment.NewLine;

        private IExamEvaluator evaluator;
        private TaskQuestion qMultiChoice, qSingleChoice, qMultiLine, qSingleLine, qPriorityAll, qPrioritySome;

        #region Setup

        [OneTimeSetUp]
        public void InitTestFixture()
        {
            this.evaluator = new ExamEvaluator();

            this.qMultiChoice = new TaskQuestion
            {
                Type = TaskQuestionType.MultiChoice,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { Text = "A1", Value = False, Score = 0 },
                    new TaskAnswer { Text = "A2", Value = True, Score = 10 },
                    new TaskAnswer { Text = "A3", Value = True, Score = 20 },
                    new TaskAnswer { Text = "A4", Value = False, Score = 0 },
                }
            };

            this.qSingleChoice = new TaskQuestion
            {
                Type = TaskQuestionType.SingleChoice,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { Text = "A1", Value = False, Score = 0 },
                    new TaskAnswer { Text = "A2", Value = True, Score = 10 },
                    new TaskAnswer { Text = "A3", Value = False, Score = 0 },
                    new TaskAnswer { Text = "A4", Value = False, Score = 0 },
                }
            };

            string multiline = "abc" + this.NL + "123";
            this.qMultiLine = new TaskQuestion
            {
                Type = TaskQuestionType.MultiLine,
                Answers = new List<TaskAnswer> { new TaskAnswer { Text = multiline, Value = multiline, Score = 40 } }
            };

            string singleline = "100abc";
            this.qSingleLine = new TaskQuestion
            {
                Type = TaskQuestionType.SingleLine,
                Answers = new List<TaskAnswer> { new TaskAnswer { Text = singleline, Value = singleline, Score = 10 } }
            };

            this.qPriorityAll = new TaskQuestion
            {
                Type = TaskQuestionType.PrioritySelection,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { Text = "A1", Value = "1", Score = 1 },
                    new TaskAnswer { Text = "A2", Value = "2", Score = 2 },
                    new TaskAnswer { Text = "A3", Value = "3", Score = 10 },
                    new TaskAnswer { Text = "A4", Value = "4", Score = 20 },
                }
            };

            this.qPrioritySome = new TaskQuestion
            {
                Type = TaskQuestionType.PrioritySelection,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { Text = "A1", Value = "1", Score = 1 },
                    new TaskAnswer { Text = "A2", Value = "2", Score = 2 },
                    new TaskAnswer { Text = "A3", Value = "3", Score = 10 },
                    new TaskAnswer { Text = "A4", Value = "4", Score = 20 },
                    new TaskAnswer { Text = "A5", Value = None, Score = 50 },
                    new TaskAnswer { Text = "A6", Value = None, Score = 100 },
                }
            };
        }

        [SetUp]
        public void InitTestCase()
        {
        }

        #endregion

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
                Task = new ExamTask { PassingThreshold = 50, },
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
        public void EvalEmptyAttempt()
        {
            var attempt = new ExamAttempt
            {
                Task = new ExamTask { PassingThreshold = 50, },
                Questions = new List<AttemptQuestion>()
            };
            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.IsPassed);
            Assert.AreEqual(0, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        #region Multi Choice

        [TestCase]
        public void EvalMultichoiceQuestion_AllAnswersCorrect()
        {
            var A = this.qMultiChoice.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = False },
                new AttemptAnswer { TaskAnswer = A[1], Value = True },
                new AttemptAnswer { TaskAnswer = A[2], Value = True },
                new AttemptAnswer { TaskAnswer = A[3], Value = False },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qMultiChoice, answers);

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.Questions[0].IsAccepted);
            Assert.True(attempt.Questions[0].IsPerfect);
            Assert.True(attempt.IsPassed);
            Assert.AreEqual(30, attempt.MaxScore);
            Assert.AreEqual(30, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultichoiceQuestion_AllAnswersFalse()
        {
            var qMulti = new TaskQuestion
            {
                Type = TaskQuestionType.MultiChoice,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { Text = "1", Value = False, Score = 1 },
                    new TaskAnswer { Text = "2", Value = False, Score = 2 },
                    new TaskAnswer { Text = "3", Value = False, Score = 3 },
                }
            };
            var A = qMulti.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = False },
                new AttemptAnswer { TaskAnswer = A[1], Value = False },
                new AttemptAnswer { TaskAnswer = A[2], Value = False },
            };

            var attempt = this.MakeSingleQuestionAttempt(qMulti, answers);

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.Questions[0].IsAccepted);
            Assert.True(attempt.Questions[0].IsPerfect);
            Assert.True(attempt.IsPassed);
            Assert.AreEqual(6, attempt.MaxScore);
            Assert.AreEqual(6, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultichoiceQuestion_AllAnswersFalseIncorrectValue()
        {
            var qMulti = new TaskQuestion
            {
                Type = TaskQuestionType.MultiChoice,
                Answers = new List<TaskAnswer>
                {
                    new TaskAnswer { Text = "1", Value = False, Score = 1 },
                    new TaskAnswer { Text = "2", Value = False, Score = 2 },
                    new TaskAnswer { Text = "3", Value = False, Score = 3 },
                }
            };
            var A = qMulti.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = True },
                new AttemptAnswer { TaskAnswer = A[1], Value = False },
                new AttemptAnswer { TaskAnswer = A[2], Value = False },
            };

            var attempt = this.MakeSingleQuestionAttempt(qMulti, answers);

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(6, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultichoiceQuestion_NoAnswers()
        {
            var A = this.qMultiChoice.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = False },
                new AttemptAnswer { TaskAnswer = A[1], Value = False },
                new AttemptAnswer { TaskAnswer = A[2], Value = False },
                new AttemptAnswer { TaskAnswer = A[3], Value = False },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qMultiChoice, answers);

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(30, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultichoiceQuestion_AllCorrectAnswersAndIncorrect()
        {
            var A = this.qMultiChoice.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = False },
                new AttemptAnswer { TaskAnswer = A[1], Value = True },
                new AttemptAnswer { TaskAnswer = A[2], Value = True },
                new AttemptAnswer { TaskAnswer = A[3], Value = True },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qMultiChoice, answers);

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(30, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultichoiceQuestion_PartiallyCorrectPassed()
        {
            var A = this.qMultiChoice.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = False },
                new AttemptAnswer { TaskAnswer = A[1], Value = False },
                new AttemptAnswer { TaskAnswer = A[2], Value = True },
                new AttemptAnswer { TaskAnswer = A[3], Value = False },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qMultiChoice, answers);

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.True(attempt.IsPassed);
            Assert.AreEqual(30, attempt.MaxScore);
            Assert.AreEqual(20, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultichoiceQuestion_PartiallyCorrectFailed()
        {
            var A = this.qMultiChoice.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = False },
                new AttemptAnswer { TaskAnswer = A[1], Value = True },
                new AttemptAnswer { TaskAnswer = A[2], Value = False },
                new AttemptAnswer { TaskAnswer = A[3], Value = False },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qMultiChoice, answers);

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(30, attempt.MaxScore);
            Assert.AreEqual(10, attempt.ExamScore);
        }

        #endregion

        #region Single Choice

        [TestCase]
        public void EvalSingleChoiceQuestion_CorrectAnswer()
        {
            var A = this.qSingleChoice.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = False },
                new AttemptAnswer { TaskAnswer = A[1], Value = True },
                new AttemptAnswer { TaskAnswer = A[2], Value = False },
                new AttemptAnswer { TaskAnswer = A[3], Value = False },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qSingleChoice, answers);

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.Questions[0].IsAccepted);
            Assert.True(attempt.Questions[0].IsPerfect);
            Assert.True(attempt.IsPassed);
            Assert.AreEqual(10, attempt.MaxScore);
            Assert.AreEqual(10, attempt.ExamScore);
        }

        [TestCase]
        public void EvalSingleChoiceQuestion_NoAnswer()
        {
            var A = this.qSingleChoice.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = False },
                new AttemptAnswer { TaskAnswer = A[1], Value = False },
                new AttemptAnswer { TaskAnswer = A[2], Value = False },
                new AttemptAnswer { TaskAnswer = A[3], Value = False },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qSingleChoice, answers);

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(10, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        /// <summary>
        /// If multiple choices are provides for a single-choice question, evaluator should not accept answers
        /// </summary>
        [TestCase]
        public void EvalSingleChoiceQuestion_MultiChoiceInput()
        {
            var A = this.qSingleChoice.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = False },
                new AttemptAnswer { TaskAnswer = A[1], Value = True },
                new AttemptAnswer { TaskAnswer = A[2], Value = True },
                new AttemptAnswer { TaskAnswer = A[3], Value = False },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qSingleChoice, answers);

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(10, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        #endregion

        #region Multi Line

        [TestCase]
        public void EvalMultiLineQuestion_AllAnswersCorrect()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qMultiLine.Answers[0], Value = "abc" + this.NL + "123" };
            var attempt = MakeSingleQuestionAttempt(this.qMultiLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.Questions[0].IsAccepted);
            Assert.True(attempt.Questions[0].IsPerfect);
            Assert.True(attempt.IsPassed);
            Assert.AreEqual(40, attempt.MaxScore);
            Assert.AreEqual(40, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultiLineQuestion_NoAnswers()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qMultiLine.Answers[0], Value = null };
            var attempt = MakeSingleQuestionAttempt(this.qMultiLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(40, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultiLineQuestion_NoFirstAnswer()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qMultiLine.Answers[0], Value = this.NL + "123" };
            var attempt = MakeSingleQuestionAttempt(this.qMultiLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.True(attempt.IsPassed);
            Assert.AreEqual(40, attempt.MaxScore);
            Assert.AreEqual(20, attempt.ExamScore);
        }

        /// <summary>
        /// Evaluator should ignore: casing; whites spaces at the start/end of each line; trailing line breaks
        /// </summary>
        [TestCase]
        public void EvalMultiLineQuestion_AllAnswersFormatInsensitive()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qMultiLine.Answers[0], Value = " ABC " + this.NL + "\t123 " + this.NL + this.NL + this.NL };
            var attempt = MakeSingleQuestionAttempt(this.qMultiLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.Questions[0].IsAccepted);
            Assert.True(attempt.Questions[0].IsPerfect);
            Assert.True(attempt.IsPassed);
            Assert.AreEqual(40, attempt.MaxScore);
            Assert.AreEqual(40, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultiLineQuestion_AllAnswersUnexpectedLineBreak()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qMultiLine.Answers[0], Value = "abc" + this.NL + this.NL + "123" };
            var attempt = MakeSingleQuestionAttempt(this.qMultiLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.True(attempt.IsPassed);
            Assert.AreEqual(40, attempt.MaxScore);
            Assert.AreEqual(20, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultiLineQuestion_AllAnswersIncorrectOrder()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qMultiLine.Answers[0], Value = "123" + this.NL + "abc" };
            var attempt = MakeSingleQuestionAttempt(this.qMultiLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(40, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultiLineQuestion_AllAnswersIncorrectInput()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qMultiLine.Answers[0], Value = "abcabc" + this.NL + "123123" };
            var attempt = MakeSingleQuestionAttempt(this.qMultiLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(40, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultiLineQuestion_CorrectAndIncorrectAnswer()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qMultiLine.Answers[0], Value = "abc" + this.NL + "123123" };
            var attempt = MakeSingleQuestionAttempt(this.qMultiLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.True(attempt.IsPassed);
            Assert.AreEqual(40, attempt.MaxScore);
            Assert.AreEqual(20, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultiLineQuestion_AllCorrectAnswersAndNoiseInput()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qMultiLine.Answers[0], Value = "abc" + this.NL + "123" + this.NL + " smth " };
            var attempt = MakeSingleQuestionAttempt(this.qMultiLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.Questions[0].IsAccepted);
            Assert.True(attempt.Questions[0].IsPerfect);
            Assert.True(attempt.IsPassed);
            Assert.AreEqual(40, attempt.MaxScore);
            Assert.AreEqual(40, attempt.ExamScore);
        }

        [Test]
        public void EvalMultiLineQuestion_RepeatedAnswer()
        {
            string repeated = False + this.NL + False + this.NL + False;
            var qRepeated = new TaskQuestion
            {
                Type = TaskQuestionType.MultiLine,
                Answers = new List<TaskAnswer> { new TaskAnswer { Text = repeated, Value = repeated, Score = 30 } }
            };

            var answer = new AttemptAnswer { TaskAnswer = qRepeated.Answers[0], Value = False + this.NL };
            var attempt = MakeSingleQuestionAttempt(qRepeated, answer);

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(30, attempt.MaxScore);
            Assert.AreEqual(10, attempt.ExamScore);
        }

        #endregion

        #region Single Line

        [TestCase]
        public void EvalSingleLineQuestion_CorrectAnswer()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qSingleLine.Answers[0], Value = "100abc" };
            var attempt = this.MakeSingleQuestionAttempt(this.qSingleLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.Questions[0].IsAccepted);
            Assert.True(attempt.Questions[0].IsPerfect);
            Assert.True(attempt.IsPassed);
            Assert.AreEqual(10, attempt.MaxScore);
            Assert.AreEqual(10, attempt.ExamScore);
        }

        [TestCase]
        public void EvalSingleLineQuestion_NoAnswer()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qSingleLine.Answers[0], Value = null };
            var attempt = this.MakeSingleQuestionAttempt(this.qSingleLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(10, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalSingleLineQuestion_IncorrectAnswer()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qSingleLine.Answers[0], Value = "100" };
            var attempt = this.MakeSingleQuestionAttempt(this.qSingleLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(10, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalSingleLineQuestion_CorrectAnswerFormatInsensitive()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qSingleLine.Answers[0], Value = " \t100ABC " };
            var attempt = this.MakeSingleQuestionAttempt(this.qSingleLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.Questions[0].IsAccepted);
            Assert.True(attempt.Questions[0].IsPerfect);
            Assert.True(attempt.IsPassed);
            Assert.AreEqual(10, attempt.MaxScore);
            Assert.AreEqual(10, attempt.ExamScore);
        }

        [TestCase]
        public void EvalSingleLineQuestion_CorrectAnswerUnexpectedLineBreak()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qSingleLine.Answers[0], Value = "100abc" + this.NL + this.NL };
            var attempt = this.MakeSingleQuestionAttempt(this.qSingleLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.Questions[0].IsAccepted);
            Assert.True(attempt.Questions[0].IsPerfect);
            Assert.True(attempt.IsPassed);
            Assert.AreEqual(10, attempt.MaxScore);
            Assert.AreEqual(10, attempt.ExamScore);
        }

        #endregion

        #region Priority

        [TestCase]
        public void EvalPriority_AllOptionsAllCorrect()
        {
            var A = this.qPriorityAll.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = "1" },
                new AttemptAnswer { TaskAnswer = A[1], Value = "2" },
                new AttemptAnswer { TaskAnswer = A[2], Value = "3" },
                new AttemptAnswer { TaskAnswer = A[3], Value = "4" },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qPriorityAll, answers);

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.Questions[0].IsAccepted);
            Assert.True(attempt.Questions[0].IsPerfect);
            Assert.True(attempt.IsPassed);
            Assert.AreEqual(33, attempt.MaxScore);
            Assert.AreEqual(33, attempt.ExamScore);
        }

        [TestCase]
        public void EvalPriority_SomeOptionsAllCorrect()
        {
            var A = this.qPrioritySome.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = "1" },
                new AttemptAnswer { TaskAnswer = A[1], Value = "2" },
                new AttemptAnswer { TaskAnswer = A[2], Value = "3" },
                new AttemptAnswer { TaskAnswer = A[3], Value = "4" },
                new AttemptAnswer { TaskAnswer = A[4], Value = None },
                new AttemptAnswer { TaskAnswer = A[5], Value = None },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qPrioritySome, answers);

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.Questions[0].IsAccepted);
            Assert.True(attempt.Questions[0].IsPerfect);
            Assert.True(attempt.IsPassed);
            Assert.AreEqual(183, attempt.MaxScore);
            Assert.AreEqual(183, attempt.ExamScore);
        }

        [TestCase]
        public void EvalPriority_SomeOptionsAllCorrectAndWrongSelection()
        {
            var A = this.qPrioritySome.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = "1" },
                new AttemptAnswer { TaskAnswer = A[1], Value = "2" },
                new AttemptAnswer { TaskAnswer = A[2], Value = "3" },
                new AttemptAnswer { TaskAnswer = A[3], Value = "4" },
                new AttemptAnswer { TaskAnswer = A[4], Value = "5" },
                new AttemptAnswer { TaskAnswer = A[5], Value = None },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qPrioritySome, answers);

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(183, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalPriority_AllOptionsNoAnswers()
        {
            var A = this.qPriorityAll.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = None },
                new AttemptAnswer { TaskAnswer = A[1], Value = None },
                new AttemptAnswer { TaskAnswer = A[2], Value = None },
                new AttemptAnswer { TaskAnswer = A[3], Value = None },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qPriorityAll, answers);

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(33, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalPriority_SomeOptionsNoAnswers()
        {
            var A = this.qPrioritySome.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = None },
                new AttemptAnswer { TaskAnswer = A[1], Value = None },
                new AttemptAnswer { TaskAnswer = A[2], Value = None },
                new AttemptAnswer { TaskAnswer = A[3], Value = None },
                new AttemptAnswer { TaskAnswer = A[4], Value = None },
                new AttemptAnswer { TaskAnswer = A[5], Value = None },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qPrioritySome, answers);

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(183, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalPriority_AllOptionsAllIncorrect()
        {
            var A = this.qPriorityAll.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = "4" },
                new AttemptAnswer { TaskAnswer = A[1], Value = "3" },
                new AttemptAnswer { TaskAnswer = A[2], Value = "2" },
                new AttemptAnswer { TaskAnswer = A[3], Value = "1" },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qPriorityAll, answers);

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(33, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalPriority_SomeOptionsAllIncorrect()
        {
            var A = this.qPrioritySome.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = "4" },
                new AttemptAnswer { TaskAnswer = A[1], Value = "3" },
                new AttemptAnswer { TaskAnswer = A[2], Value = "2" },
                new AttemptAnswer { TaskAnswer = A[3], Value = "1" },
                new AttemptAnswer { TaskAnswer = A[4], Value = None },
                new AttemptAnswer { TaskAnswer = A[5], Value = None },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qPrioritySome, answers);

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(183, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalPriority_AllOptionsRepeatedSelection()
        {
            var A = this.qPriorityAll.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = "1" },
                new AttemptAnswer { TaskAnswer = A[1], Value = "2" },
                new AttemptAnswer { TaskAnswer = A[2], Value = "2" },
                new AttemptAnswer { TaskAnswer = A[3], Value = "2" },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qPriorityAll, answers);

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(33, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalPriority_SomeOptionsRepeatedSelection()
        {
            var A = this.qPrioritySome.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = "1" },
                new AttemptAnswer { TaskAnswer = A[1], Value = "2" },
                new AttemptAnswer { TaskAnswer = A[2], Value = "2" },
                new AttemptAnswer { TaskAnswer = A[3], Value = "2" },
                new AttemptAnswer { TaskAnswer = A[4], Value = None },
                new AttemptAnswer { TaskAnswer = A[5], Value = None },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qPrioritySome, answers);

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(183, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalPriority_AllOptionsStartsCorrect()
        {
            var A = this.qPriorityAll.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = "1" },
                new AttemptAnswer { TaskAnswer = A[1], Value = "3" },
                new AttemptAnswer { TaskAnswer = A[2], Value = "2" },
                new AttemptAnswer { TaskAnswer = A[3], Value = "4" },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qPriorityAll, answers);

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(33, attempt.MaxScore);
            Assert.AreEqual(1, attempt.ExamScore);
        }

        [TestCase]
        public void EvalPriority_SomeOptionsStartsCorrect()
        {
            var A = this.qPrioritySome.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = "1" },
                new AttemptAnswer { TaskAnswer = A[1], Value = "3" },
                new AttemptAnswer { TaskAnswer = A[2], Value = "2" },
                new AttemptAnswer { TaskAnswer = A[3], Value = "4" },
                new AttemptAnswer { TaskAnswer = A[4], Value = None },
                new AttemptAnswer { TaskAnswer = A[5], Value = None },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qPrioritySome, answers);

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.True(attempt.IsPassed);
            Assert.AreEqual(183, attempt.MaxScore);
            Assert.AreEqual(151, attempt.ExamScore);
        }

        [TestCase]
        public void EvalPriority_SomeOptionsStartsCorrectWrongSelection()
        {
            var A = this.qPrioritySome.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = "1" },
                new AttemptAnswer { TaskAnswer = A[1], Value = "3" },
                new AttemptAnswer { TaskAnswer = A[2], Value = "2" },
                new AttemptAnswer { TaskAnswer = A[3], Value = "4" },
                new AttemptAnswer { TaskAnswer = A[4], Value = "5" },
                new AttemptAnswer { TaskAnswer = A[5], Value = None },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qPrioritySome, answers);

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(183, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalPriority_AllOptionsStartsIncorrect()
        {
            var A = this.qPriorityAll.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = "2" },
                new AttemptAnswer { TaskAnswer = A[1], Value = "1" },
                new AttemptAnswer { TaskAnswer = A[2], Value = "3" },
                new AttemptAnswer { TaskAnswer = A[3], Value = "4" },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qPriorityAll, answers);

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(33, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalPriority_SomeOptionsStartsIncorrect()
        {
            var A = this.qPrioritySome.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = "2" },
                new AttemptAnswer { TaskAnswer = A[1], Value = "1" },
                new AttemptAnswer { TaskAnswer = A[2], Value = "3" },
                new AttemptAnswer { TaskAnswer = A[3], Value = "4" },
                new AttemptAnswer { TaskAnswer = A[4], Value = None },
                new AttemptAnswer { TaskAnswer = A[5], Value = None },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qPrioritySome, answers);

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(183, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalPriority_SomeOptionsStartsIncorrectWrongSelection()
        {
            var A = this.qPrioritySome.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = "2" },
                new AttemptAnswer { TaskAnswer = A[1], Value = "1" },
                new AttemptAnswer { TaskAnswer = A[2], Value = "3" },
                new AttemptAnswer { TaskAnswer = A[3], Value = "4" },
                new AttemptAnswer { TaskAnswer = A[4], Value = "5" },
                new AttemptAnswer { TaskAnswer = A[5], Value = None },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qPrioritySome, answers);

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(183, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalPriority_AllOptionsStartsCorrectEndsNoSelection()
        {
            var A = this.qPriorityAll.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = "1" },
                new AttemptAnswer { TaskAnswer = A[1], Value = None },
                new AttemptAnswer { TaskAnswer = A[2], Value = "3" },
                new AttemptAnswer { TaskAnswer = A[3], Value = None },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qPriorityAll, answers);

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.Questions[0].IsAccepted);
            Assert.False(attempt.Questions[0].IsPerfect);
            Assert.False(attempt.IsPassed);
            Assert.AreEqual(33, attempt.MaxScore);
            Assert.AreEqual(1, attempt.ExamScore);
        }

        #endregion

        private ExamAttempt MakeAttemptTemplate()
        {
            return new ExamAttempt
            {
                Task = new ExamTask { PassingThreshold = 50, },
                Questions = new List<AttemptQuestion>
                {
                    new AttemptQuestion
                    {
                        TaskQuestion = new TaskQuestion { Type = TaskQuestionType.MultiChoice, },
                        AttemptAnswers = new List<AttemptAnswer>
                        {
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = False, Score = 0 }, Value = False },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True,  Score = 1 }, Value = False },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = False, Score = 0 }, Value = False },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True,  Score = 1 }, Value = False },
                        }
                    },

                    new AttemptQuestion
                    {
                        TaskQuestion = new TaskQuestion { Type = TaskQuestionType.SingleChoice, },
                        AttemptAnswers = new List<AttemptAnswer>
                        {
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True,  Score = 1 }, Value = False },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = False, Score = 0 }, Value = False },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = False, Score = 0 }, Value = False },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = False, Score = 0 }, Value = False },
                        }
                    },

                    new AttemptQuestion
                    {
                        TaskQuestion = new TaskQuestion { Type = TaskQuestionType.MultiLine, },
                        AttemptAnswers = new List<AttemptAnswer>
                        {
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = "1" + this.NL+ "2" + this.NL + "3", Score = 3 }, Value = null },
                        }
                    },

                    new AttemptQuestion
                    {
                        TaskQuestion = new TaskQuestion { Type = TaskQuestionType.SingleLine, },
                        AttemptAnswers = new List<AttemptAnswer>
                        {
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = "z", Score = 2 }, Value = null },
                        }
                    },
                }
            };
        }

        [TestCase]
        public void EvalAttempt_NoAnswers()
        {
            var attempt = MakeAttemptTemplate();

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.IsPassed);
            Assert.False(attempt.Questions.Any(q => q.IsAccepted == true));
            Assert.AreEqual(8, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalAttempt_AllAnswersCorrect()
        {
            var attempt = MakeAttemptTemplate();

            foreach (var question in attempt.Questions)
            {
                foreach (var a in question.AttemptAnswers)
                {
                    a.Value = a.TaskAnswer.Value;
                }
            }

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.IsPassed);
            Assert.True(attempt.Questions.All(q => q.IsAccepted == true));
            Assert.AreEqual(8, attempt.MaxScore);
            Assert.AreEqual(8, attempt.ExamScore);
        }

        [TestCase]
        public void EvalAttempt_PartiallyCorrectPassed()
        {
            var attempt = MakeAttemptTemplate();

            Func<AttemptQuestion, bool> questionFilter = q => q.TaskQuestion.Type == TaskQuestionType.MultiLine || q.TaskQuestion.Type == TaskQuestionType.SingleLine;

            foreach (var question in attempt.Questions.Where(questionFilter))
            {
                foreach (var a in question.AttemptAnswers)
                {
                    a.Value = a.TaskAnswer.Value;
                }
            }

            this.evaluator.Evaluate(attempt);

            Assert.True(attempt.IsPassed);
            Assert.True(attempt.Questions.All(q => q.IsAccepted == questionFilter(q)));
            Assert.AreEqual(8, attempt.MaxScore);
            Assert.AreEqual(5, attempt.ExamScore);
        }

        [TestCase]
        public void EvalAttempt_PartiallyCorrectFailed()
        {
            var attempt = MakeAttemptTemplate();

            Func<AttemptQuestion, bool> questionFilter = q => q.TaskQuestion.Type == TaskQuestionType.MultiChoice || q.TaskQuestion.Type == TaskQuestionType.SingleChoice;

            foreach (var question in attempt.Questions.Where(questionFilter))
            {
                foreach (var a in question.AttemptAnswers)
                {
                    a.Value = a.TaskAnswer.Value;
                }
            }

            this.evaluator.Evaluate(attempt);

            Assert.False(attempt.IsPassed);
            Assert.True(attempt.Questions.All(q => q.IsAccepted == questionFilter(q)));
            Assert.AreEqual(8, attempt.MaxScore);
            Assert.AreEqual(3, attempt.ExamScore);
        }

        #region Threshold

        [TestCase]
        public void EvalAttempt_NonDefaultThreshold_100PercentThresholdPassed()
        {
            var attempt = new ExamAttempt
            {
                Task = new ExamTask { PassingThreshold = 100 },
                Questions = new List<AttemptQuestion>
                {
                    new AttemptQuestion
                    {
                        TaskQuestion = new TaskQuestion { Type = TaskQuestionType.MultiChoice, },
                        AttemptAnswers =
                            Enumerable.Range(1,4)
                            .Select(i => new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True, Score = 1 }, Value = True })
                            .ToList(),
                    }
                }
            };

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(4, attempt.MaxScore);
            Assert.AreEqual(4, attempt.ExamScore);
            Assert.True(attempt.IsPassed);
        }

        [TestCase]
        public void EvalAttempt_NonDefaultThreshold_HighThresholdFailed()
        {
            var attempt = new ExamAttempt
            {
                Task = new ExamTask { PassingThreshold = 80 },
                Questions = new List<AttemptQuestion>
                {
                    new AttemptQuestion
                    {
                        TaskQuestion = new TaskQuestion { Type = TaskQuestionType.MultiChoice, },
                        AttemptAnswers = new List<AttemptAnswer>
                        {
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True, Score = 1 }, Value = True },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True, Score = 1 }, Value = True },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True, Score = 1 }, Value = True },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True, Score = 1 }, Value = False },
                        }
                    }
                }
            };

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(4, attempt.MaxScore);
            Assert.AreEqual(3, attempt.ExamScore);
            Assert.False(attempt.IsPassed);
        }

        [TestCase]
        public void EvalAttempt_NonDefaultThreshold_HighThresholdPassed()
        {
            var attempt = new ExamAttempt
            {
                Task = new ExamTask { PassingThreshold = 75 },
                Questions = new List<AttemptQuestion>
                {
                    new AttemptQuestion
                    {
                        TaskQuestion = new TaskQuestion { Type = TaskQuestionType.MultiChoice, },
                        AttemptAnswers = new List<AttemptAnswer>
                        {
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True, Score = 1 }, Value = True },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True, Score = 1 }, Value = True },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True, Score = 1 }, Value = True },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True, Score = 1 }, Value = True },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True, Score = 1 }, Value = False },
                        }
                    }
                }
            };

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(5, attempt.MaxScore);
            Assert.AreEqual(4, attempt.ExamScore);
            Assert.True(attempt.IsPassed);
        }

        [TestCase]
        public void EvalAttempt_NonDefaultThreshold_LowThresholdPassed()
        {
            var attempt = new ExamAttempt
            {
                Task = new ExamTask { PassingThreshold = 33 },
                Questions = new List<AttemptQuestion>
                {
                    new AttemptQuestion
                    {
                        TaskQuestion = new TaskQuestion { Type = TaskQuestionType.MultiChoice, },
                        AttemptAnswers = new List<AttemptAnswer>
                        {
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True, Score = 1 }, Value = True },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True, Score = 1 }, Value = True },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True, Score = 1 }, Value = False },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True, Score = 1 }, Value = False },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True, Score = 1 }, Value = False },
                        }
                    }
                }
            };

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(5, attempt.MaxScore);
            Assert.AreEqual(2, attempt.ExamScore);
            Assert.True(attempt.IsPassed);
        }

        [TestCase]
        public void EvalAttempt_NonDefaultThreshold_LowThresholdFailed()
        {
            var attempt = new ExamAttempt
            {
                Task = new ExamTask { PassingThreshold = 25 },
                Questions = new List<AttemptQuestion>
                {
                    new AttemptQuestion
                    {
                        TaskQuestion = new TaskQuestion { Type = TaskQuestionType.MultiChoice, },
                        AttemptAnswers = new List<AttemptAnswer>
                        {
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True, Score = 1 }, Value = True },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True, Score = 1 }, Value = False },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True, Score = 1 }, Value = False },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True, Score = 1 }, Value = False },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = True, Score = 1 }, Value = False },
                        }
                    }
                }
            };

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(5, attempt.MaxScore);
            Assert.AreEqual(1, attempt.ExamScore);
            Assert.False(attempt.IsPassed);
        }

        #endregion 
    }
}
