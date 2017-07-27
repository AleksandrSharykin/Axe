using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    public class SkillAssessment
    {
        public int Id { get; set; }

        public string StudentId { get; set; }

        public ApplicationUser Student { get; set; }

        public string ExaminerId { get; set; }

        public ApplicationUser Examiner { get; set; }

        public int TechnologyId { get; set; }

        public Technology Technology { get; set; }

        public DateTime? ExamDate { get; set; }

        public int? ExamScore { get; set; }

        public bool? IsPassed { get; set; }
    }
}
