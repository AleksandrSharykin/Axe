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
            ExamTask task = attempt.Task;

            attempt.Technology = task.Technology;

            attempt.MaxScore = attempt.Questions.SelectMany(q => q.TaskQuestion.Answers).Sum(a => a.Score);
            attempt.ExamScore = 0;

            // evaluate each question
            foreach (var question in attempt.Questions)
            {
                bool isQuestionAccepted = true;
                question.Score = 0;

                if (question.TaskQuestion.Type == TaskQuestionType.MultiLine)
                {
                    var answer = question.AttemptAnswers[0];

                    var userValues = SplitInput(answer.Value);
                    var correctValues = SplitInput(answer.TaskAnswer.Value);

                    // compare provided answers line-by-line with correct answers
                    // answers order is important
                    var matchCount = userValues.Zip(correctValues, (userValue, correctValue) => userValue == correctValue ? 1 : 0).Sum();                    

                    question.IsAccepted = matchCount > 0;

                    if (question.IsAccepted == true)
                    {                        
                        if (matchCount == correctValues.Count)
                        {
                            // award full points for all correct answers
                            question.Score = answer.TaskAnswer.Score;
                        }
                        else
                        {
                            // award part of points 
                            question.Score = (int)Math.Floor(answer.TaskAnswer.Score * matchCount / (float)correctValues.Count);
                        }
                    }                    
                }
                else
                {
                    // compare user answers with correct answers
                    foreach (var ap in question.AttemptAnswers)
                    {
                        var attemptAnswer = ap.Value?.ToLower() ?? String.Empty;
                        var taskAnswer = ap.TaskAnswer.Value?.ToLower() ?? String.Empty;
                        if (attemptAnswer == taskAnswer)
                        {
                            question.Score += ap.TaskAnswer.Score;
                        }
                        else
                        {
                            isQuestionAccepted = false;
                        }
                    }

                    // if incorrect answer is selected, points are not awarded
                    // award points for correct answers (full score for all answers)
                    question.IsAccepted = isQuestionAccepted || 
                        question.AttemptAnswers.Where(a => a.Value == Boolean.TrueString).All(p => p.TaskAnswer.Value == Boolean.TrueString);
                }

                if (question.IsAccepted == true)
                {                    
                    attempt.ExamScore += question.Score;                    
                }
                else
                {
                    question.Score = 0;
                }
            }

            // todo : set threshold in Task editor
            // threshold 50%
            attempt.IsPassed = attempt.ExamScore > 0.5 * attempt.MaxScore;
        }

        private IList<string> SplitInput(string answer)
        {
            return (answer ?? String.Empty)
                    .ToLower()
                    .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                    .Select(s => s.Trim())
                    .ToList();
        }
    }
}
