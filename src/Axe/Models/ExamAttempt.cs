using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    public class ExamAttempt
    {
        public int Id { get; set;}

        public int? TechnologyId { get; set; }

        public Technology Technology { get; set; }

        public int TaskId { get; set; }

        public ExamTask Task { get; set; }

        public string StudentId { get; set; }

        public ApplicationUser Student { get; set; }

        public DateTime? ExamDate { get; set; }

        public int? ExamScore { get; set; }

        public bool? IsPassed { get; set; }

        public IList<AttemptQuestion> Questions { get; set; }

        public int? MaxScore { get; set; }        
    }
}
