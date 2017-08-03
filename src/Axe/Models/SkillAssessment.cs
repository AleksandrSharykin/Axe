using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

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

        [DisplayFormat(DataFormatString ="{0:dd.MM.yyy HH:mm}")]
        [Display(Name = "Date")]
        public DateTime? ExamDate { get; set; }
        
        [Range(0,100)]        
        public int? ExamScore { get; set; }

        [Display(Name = "Comment")]
        [MaxLength(1000)]
        public string ExamComment { get; set; }

        [Display(Name = "Exam passed")]
        public bool? IsPassed { get; set; }
    }
}
