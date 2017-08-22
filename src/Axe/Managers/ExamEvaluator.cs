using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Axe.Models;

namespace Axe.Managers
{
    /// <summary>
    /// Class evaluates exam results
    /// </summary>
    public class ExamEvaluator : IExamEvaluator
    {
        /// <summary>
        /// Checks answers correctness and calculates score
        /// </summary>
        /// <param name="attempt"></param>
        public void Evaluate(ExamAttempt attempt)
        {
            attempt.MaxScore = attempt.Questions.SelectMany(q => q.AttemptAnswers.Select(a => a.TaskAnswer)).Sum(a => a.Score);
            attempt.ExamScore = 0;

            // evaluate each question
            foreach (var question in attempt.Questions)
            {
                int score = 0;

                if (question.TaskQuestion.Type == TaskQuestionType.MultiLine)
                {
                    var answer = question.AttemptAnswers[0];

                    var userValues = SplitInput(answer.Value);
                    var correctValues = SplitInput(answer.TaskAnswer.Value);

                    // compare provided answers line-by-line with correct answers
                    // answers order is important
                    var matchCount = userValues.Zip(correctValues, (userValue, correctValue) => userValue == correctValue ? 1 : 0).Sum();

                    question.IsAccepted = matchCount > 0;

                    question.IsPerfect = question.IsAccepted.Value && matchCount == correctValues.Count;

                    if (true == question.IsAccepted)
                    {
                        if (true == question.IsPerfect)
                        {
                            // award full points for all correct answers
                            score = answer.TaskAnswer.Score;
                        }
                        else
                        {
                            // award part of points for some correct answers
                            score = (int)Math.Floor(answer.TaskAnswer.Score * matchCount / (float)correctValues.Count);
                        }
                    }
                }
                else if (question.TaskQuestion.Type == TaskQuestionType.PrioritySelection)
                {
                    // all options are selected correctly
                    question.IsPerfect = question.AttemptAnswers
                            .All(answer => Normalize(answer.Value) == Normalize(answer.TaskAnswer.Value));

                    if (true == question.IsPerfect)
                    {
                        question.IsAccepted = true;
                        score = question.AttemptAnswers.Sum(a => a.TaskAnswer.Score);
                    }
                    else
                    {
                        var hasWrongSelection =
                            // user selected options which should not have been selected
                            question.AttemptAnswers.Any(a => a.Value != null && a.TaskAnswer.Value == null) ||
                            // user doens't know the answer and attempts to hack some points, e.g. selecting 1 for all options
                            question.AttemptAnswers.Where(a => a.Value != null).GroupBy(a => a.Value).Any(g => g.Count() > 1);

                        if (hasWrongSelection)
                        {
                            question.IsAccepted = false;
                        }
                        else
                        {
                            // correctly selected options
                            var matches = question.AttemptAnswers
                                .Where(a => a.TaskAnswer.Value != null)
                                .OrderBy(a => int.Parse(a.TaskAnswer.Value))
                                .TakeWhile(a => Normalize(a.Value) == Normalize(a.TaskAnswer.Value))
                                .ToList();

                            // accept question if at least one options is selected
                            question.IsAccepted = matches.Count > 0;

                            if (true == question.IsAccepted)
                            {
                                // award points for correct selected options
                                score += matches.Sum(a => a.TaskAnswer.Score);

                                // adding award points for all correctly not selected options only if there is correc                                
                                score += question.AttemptAnswers
                                    .Where(a => a.TaskAnswer.Value == null)
                                    .Sum(a => a.TaskAnswer.Score);
                            }
                        }
                    }
                }
                else
                {
                    // compare user answers with correct answers and award points for correct answers
                    score = question.AttemptAnswers
                                    .Select(answer => Normalize(answer.Value) == Normalize(answer.TaskAnswer.Value) ? answer.TaskAnswer.Score : 0)
                                    .Sum();

                    // if incorrect answer is selected, points for question are not awarded                    
                    question.IsAccepted = score > 0 &&
                        question.AttemptAnswers.Where(a => a.Value == Boolean.TrueString).All(p => p.TaskAnswer.Value == Boolean.TrueString);

                    question.IsPerfect = question.AttemptAnswers
                                    .All(answer => Normalize(answer.Value) == Normalize(answer.TaskAnswer.Value));
                }

                question.Score = question.IsAccepted == true ? score : 0;
                attempt.ExamScore += question.Score;
            }

            var threshold = attempt.Task.PassingThreshold * 0.01;

            attempt.IsPassed = attempt.ExamScore >= threshold * attempt.MaxScore;
        }

        /// <summary>
        /// Formats answer string for evaluation
        /// </summary>
        /// <param name="answer"></param>
        /// <returns></returns>
        private string Normalize(string answer)
        {
            return (answer ?? String.Empty).TrimStart(' ', '\t').TrimEnd().ToLower();
        }

        /// <summary>
        /// Splits a string with linebreaks into separate lines
        /// </summary>
        /// <param name="answer"></param>
        /// <returns></returns>
        private IList<string> SplitInput(string answer)
        {
            return Normalize(answer)
                    .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                    .Select(s => s.Trim())
                    .ToList();
        }
    }
}
