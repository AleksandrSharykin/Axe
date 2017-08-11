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

        /// <summary>
        /// Shortcut for line break sequence
        /// </summary>
        private readonly string NL = Environment.NewLine;

        private IExamEvaluator evaluator;
        private TaskQuestion qMultiChoice, qSingleChoice, qMultiLine, qRepeated, qSingleLine;

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

            string multiline = "abc" + this.NL + "123";
            this.qMultiLine = new TaskQuestion
            {
                Type = TaskQuestionType.MultiLine,
                Answers = new List<TaskAnswer> { new TaskAnswer { Text = multiline, Value = multiline, Score = 40 } }
            };

            string repeated = this.False + this.NL + this.False + this.NL + this.False;
            this.qRepeated = new TaskQuestion
            {
                Type = TaskQuestionType.MultiLine,
                Answers = new List<TaskAnswer> { new TaskAnswer { Text = repeated, Value = repeated, Score = 30 } }
            };

            string singleline = "100abc";
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

            Assert.AreEqual(false, attempt.IsPassed);
            Assert.AreEqual(0, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
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

        [TestCase]
        public void EvalSingleChoiceQuestion_CorrectAnswer()
        {
            var A = this.qSingleChoice.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = this.False },
                new AttemptAnswer { TaskAnswer = A[1], Value = this.True },
                new AttemptAnswer { TaskAnswer = A[2], Value = this.False },
                new AttemptAnswer { TaskAnswer = A[3], Value = this.False },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qSingleChoice, answers);

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(true, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(true, attempt.IsPassed);
            Assert.AreEqual(10, attempt.MaxScore);
            Assert.AreEqual(10, attempt.ExamScore);
        }

        [TestCase]
        public void EvalSingleChoiceQuestion_NoAnswer()
        {
            var A = this.qSingleChoice.Answers;
            var answers = new AttemptAnswer[]
            {
                new AttemptAnswer { TaskAnswer = A[0], Value = this.False },
                new AttemptAnswer { TaskAnswer = A[1], Value = this.False },
                new AttemptAnswer { TaskAnswer = A[2], Value = this.False },
                new AttemptAnswer { TaskAnswer = A[3], Value = this.False },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qSingleChoice, answers);

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(false, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(false, attempt.IsPassed);
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
                new AttemptAnswer { TaskAnswer = A[0], Value = this.False },
                new AttemptAnswer { TaskAnswer = A[1], Value = this.True },
                new AttemptAnswer { TaskAnswer = A[2], Value = this.True },
                new AttemptAnswer { TaskAnswer = A[3], Value = this.False },
            };

            var attempt = this.MakeSingleQuestionAttempt(this.qSingleChoice, answers);

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(false, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(false, attempt.IsPassed);
            Assert.AreEqual(10, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultiLineQuestion_AllAnswersCorrect()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qMultiLine.Answers[0], Value = "abc" + this.NL + "123" };
            var attempt = MakeSingleQuestionAttempt(this.qMultiLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(true, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(true, attempt.IsPassed);
            Assert.AreEqual(40, attempt.MaxScore);
            Assert.AreEqual(40, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultiLineQuestion_NoAnswers()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qMultiLine.Answers[0], Value = null };
            var attempt = MakeSingleQuestionAttempt(this.qMultiLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(false, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(false, attempt.IsPassed);
            Assert.AreEqual(40, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
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

            Assert.AreEqual(true, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(true, attempt.IsPassed);
            Assert.AreEqual(40, attempt.MaxScore);
            Assert.AreEqual(40, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultiLineQuestion_AllAnswersUnexpectedLineBreak()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qMultiLine.Answers[0], Value = "abc" + this.NL + this.NL + "123" };
            var attempt = MakeSingleQuestionAttempt(this.qMultiLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(true, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(false, attempt.IsPassed);
            Assert.AreEqual(40, attempt.MaxScore);
            Assert.AreEqual(20, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultiLineQuestion_AllAnswersIncorrectOrder()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qMultiLine.Answers[0], Value = "123" + this.NL + "abc" };
            var attempt = MakeSingleQuestionAttempt(this.qMultiLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(false, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(false, attempt.IsPassed);
            Assert.AreEqual(40, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultiLineQuestion_AllAnswersIncorrectInput()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qMultiLine.Answers[0], Value = "abcabc" + this.NL + "123123" };
            var attempt = MakeSingleQuestionAttempt(this.qMultiLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(false, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(false, attempt.IsPassed);
            Assert.AreEqual(40, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultiLineQuestion_CorrectAndIncorrectAnswer()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qMultiLine.Answers[0], Value = "abc" + this.NL + "123123" };
            var attempt = MakeSingleQuestionAttempt(this.qMultiLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(true, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(false, attempt.IsPassed);
            Assert.AreEqual(40, attempt.MaxScore);
            Assert.AreEqual(20, attempt.ExamScore);
        }

        [TestCase]
        public void EvalMultiLineQuestion_AllCorrectAnswersAndNoiseInput()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qMultiLine.Answers[0], Value = "abc" + this.NL + "123" + this.NL + " smth " };
            var attempt = MakeSingleQuestionAttempt(this.qMultiLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(true, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(true, attempt.IsPassed);
            Assert.AreEqual(40, attempt.MaxScore);
            Assert.AreEqual(40, attempt.ExamScore);
        }

        [Test]
        public void EvalMultiLineQuestion_RepeatedAnswer()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qRepeated.Answers[0], Value = this.False + this.NL };
            var attempt = MakeSingleQuestionAttempt(qRepeated, answer);

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(true, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(false, attempt.IsPassed);
            Assert.AreEqual(30, attempt.MaxScore);
            Assert.AreEqual(10, attempt.ExamScore);
        }

        [TestCase]
        public void EvalSingleLineQuestion_CorrectAnswer()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qSingleLine.Answers[0], Value = "100abc" };
            var attempt = this.MakeSingleQuestionAttempt(this.qSingleLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(true, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(true, attempt.IsPassed);
            Assert.AreEqual(10, attempt.MaxScore);
            Assert.AreEqual(10, attempt.ExamScore);
        }

        [TestCase]
        public void EvalSingleLineQuestion_NoAnswer()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qSingleLine.Answers[0], Value = null };
            var attempt = this.MakeSingleQuestionAttempt(this.qSingleLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(false, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(false, attempt.IsPassed);
            Assert.AreEqual(10, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalSingleLineQuestion_IncorrectAnswer()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qSingleLine.Answers[0], Value = "100" };
            var attempt = this.MakeSingleQuestionAttempt(this.qSingleLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(false, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(false, attempt.IsPassed);
            Assert.AreEqual(10, attempt.MaxScore);
            Assert.AreEqual(0, attempt.ExamScore);
        }

        [TestCase]
        public void EvalSingleLineQuestion_CorrectAnswerFormatInsensitive()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qSingleLine.Answers[0], Value = " \t100ABC " };
            var attempt = this.MakeSingleQuestionAttempt(this.qSingleLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(true, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(true, attempt.IsPassed);
            Assert.AreEqual(10, attempt.MaxScore);
            Assert.AreEqual(10, attempt.ExamScore);
        }

        [TestCase]
        public void EvalSingleLineQuestion_CorrectAnswerUnexpectedLineBreak()
        {
            var answer = new AttemptAnswer { TaskAnswer = this.qSingleLine.Answers[0], Value = "100abc" + this.NL + this.NL };
            var attempt = this.MakeSingleQuestionAttempt(this.qSingleLine, answer);

            this.evaluator.Evaluate(attempt);

            Assert.AreEqual(true, attempt.Questions[0].IsAccepted);
            Assert.AreEqual(true, attempt.IsPassed);
            Assert.AreEqual(10, attempt.MaxScore);
            Assert.AreEqual(10, attempt.ExamScore);
        }

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
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = this.False, Score = 0 }, Value = this.False },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = this.True,  Score = 1 }, Value = this.False },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = this.False, Score = 0 }, Value = this.False },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = this.True,  Score = 1 }, Value = this.False },
                        }
                    },

                    new AttemptQuestion
                    {
                        TaskQuestion = new TaskQuestion { Type = TaskQuestionType.SingleChoice, },
                        AttemptAnswers = new List<AttemptAnswer>
                        {
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = this.True,  Score = 1 }, Value = this.False },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = this.False, Score = 0 }, Value = this.False },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = this.False, Score = 0 }, Value = this.False },
                            new AttemptAnswer { TaskAnswer = new TaskAnswer { Value = this.False, Score = 0 }, Value = this.False },
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

            Assert.AreEqual(false, attempt.IsPassed);
            Assert.AreEqual(false, attempt.Questions.Any(q => q.IsAccepted == true));
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

            Assert.AreEqual(true, attempt.IsPassed);
            Assert.AreEqual(true, attempt.Questions.All(q => q.IsAccepted == true));
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

            Assert.AreEqual(true, attempt.IsPassed);
            Assert.AreEqual(true, attempt.Questions.All(q => q.IsAccepted == questionFilter(q)));
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

            Assert.AreEqual(false, attempt.IsPassed);
            Assert.AreEqual(true, attempt.Questions.All(q => q.IsAccepted == questionFilter(q)));
            Assert.AreEqual(8, attempt.MaxScore);
            Assert.AreEqual(3, attempt.ExamScore);
        }
    }
}
