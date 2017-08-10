using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Axe.Models;

namespace Axe.Managers
{
    /// <summary>
    /// Inderface declares contract for exam attempts evaluation
    /// </summary>
    public interface IExamEvaluator
    {
        /// <summary>
        /// Checks exam answers correctness and calculates score
        /// </summary
        void Evaluate(ExamAttempt attempt);
    }
}
