using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Axe.Models.AssessmentsVm
{
    /// <summary>
    /// Class represents input model for assigning new <see cref="SkillAssessment"/>
    /// </summary>
    public class AssessmentInputVm
    {
        /// <summary>
        /// Gets or sets assignment identifier
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Get or sets user to take skill asessment
        /// </summary>
        [Required]
        public string StudentId { get; set; }

        /// <summary>
        /// Get or sets user to take skill asessment
        /// </summary>
        public string StudentName { get; set; }


        /// <summary>
        /// Get or sets user to examine skill asessment
        /// </summary>
        [Required]
        public string ExaminerId { get; set; }

        /// <summary>
        /// Get or sets user to examine skill asessment
        /// </summary>
        public string ExaminerName { get; set; }


        /// <summary>
        /// Get or sets technology for assessment
        /// </summary>
        [Required]
        public int TechnologyId { get; set; }

        /// <summary>
        /// Get or sets technology for assessment
        /// </summary>
        public string TechnologyName { get; set; }


        /// <summary>
        /// Get or sets assessment date
        /// </summary>
        [Required]
        [Display(Prompt = "yyyy-MM-dd")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ExamDate { get; set; }

        public string MyDate { get; set; }

        /// <summary>
        /// Get or sets assessment time
        /// </summary>
        [Required]
        [DataType(DataType.Time)]
        public DateTime? ExamTime { get; set; }
    }
}
