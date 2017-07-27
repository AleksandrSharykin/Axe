using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Axe.Models
{
    public class AttemptQuestion
    {
        public int Id { get; set; }

        public int AttemptId { get; set; }

        public ExamAttempt Attempt { get; set; }

        public int SortNumber { get; set; }

        public int? TaskQuestionId { get; set; }
        
        public TaskQuestion TaskQuestion { get; set; }

        public ICollection<AttemptAnswer> AttemptAnswers { get; set; }
    }
}
