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

            var questions = task.Questions.Select(q => q.Question).ToList();
            attempt.MaxScore = questions.SelectMany(q => q.Answers).Sum(a => a.Score);
            attempt.ExamScore = 0;

            var questionPairs = questions.Join(attempt.Questions, qt => qt.Id, qa => qa.TaskQuestionId,
                (qt, qa) => new { TaskQuestion = qt, AttemptQuestion = qa });

            // evaluate each question
            foreach (var qp in questionPairs)
            {
                var answerPairs = qp.TaskQuestion.Answers.Join(qp.AttemptQuestion.AttemptAnswers, ta => ta.Id, aa => aa.TaskAnswerId,
                    (ta, aa) => new { TaskAnswer = ta, AttemptAnswer = aa });

                bool isQuestionAccepted = true;
                int questionScore = 0;
                // compare user answers with correct answers
                foreach (var ap in answerPairs)
                {
                    var attemptAnswer = ap.AttemptAnswer.Value?.ToLower() ?? String.Empty;
                    var taskAnswer = ap.TaskAnswer.Value?.ToLower() ?? String.Empty;
                    if (attemptAnswer == taskAnswer)
                    {
                        questionScore += ap.TaskAnswer.Score;
                    }
                    else
                    {
                        isQuestionAccepted = false;
                    }
                }
                
                if (isQuestionAccepted ||
                    answerPairs.Where(p => p.AttemptAnswer.IsSelected).All(p => p.TaskAnswer.IsCorrect))
                {
                    // if incorrect answer is selected, points are not awarded
                    // award points for correct answers (full score for all answers)
                    attempt.ExamScore += questionScore;
                }
            }

            // threshold 50%
            attempt.IsPassed = attempt.ExamScore > 0.5 * attempt.MaxScore;
        }
    }
}
