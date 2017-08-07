using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Axe.Models;

namespace Axe.Managers
{
    public interface IExamEvaluator
    {
        void Evaluate(ExamAttempt attempt);
    }
}
