using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Axe.Models.AssessmentsVm
{
    public class AssessmentInputVm
    {
        public int Id { get; set; }

        public SelectList Students { get; set; }

        [Required]
        public string StudentId { get; set; }

        public SelectList Examiners { get; set; }

        [Required]
        public string ExaminerId { get; set; }

        public SelectList Technologies;

        [Required]
        public int? TechnologyId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? ExamDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public DateTime? ExamTime { get; set; }
    }
}
