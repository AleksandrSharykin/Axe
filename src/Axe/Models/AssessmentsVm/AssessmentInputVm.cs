using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Axe.Models.AssessmentsVm
{
    /// <summary>
    /// Class represents input model for assigneing new <see cref="SkillAssessment"/>
    /// </summary>
    public class AssessmentInputVm
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; }

        public string StudentName { get; set; }

        [Required]
        public string ExaminerId { get; set; }

        public string ExaminerName { get; set; }        


        [Required]
        public int TechnologyId { get; set; }

        public string TechnologyName { get; set; }


        [Required]
        [DataType(DataType.Date)]
        public DateTime? ExamDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public DateTime? ExamTime { get; set; }
    }
}
