using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Axe.Models
{
    /// <summary>
    /// Class provides information about skill assessment taken by user
    /// </summary>
    public class SkillAssessment
    {
        /// <summary>
        /// Gets or sets identifier
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets student identifier
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Get or sets student name
        /// </summary>
        public ApplicationUser Student { get; set; }


        /// <summary>
        /// Gets or sets user who assigned and marked assessment
        /// </summary>
        public string ExaminerId { get; set; }

        /// <summary>
        /// Gets or sets user who assigned and marked assessment
        /// </summary>
        public ApplicationUser Examiner { get; set; }


        /// <summary>
        /// Gets or sets <see cref="Technology"/> for assessment
        /// </summary>
        public int TechnologyId { get; set; }

        /// <summary>
        /// Gets or sets <see cref="Technology"/> for assessment
        /// </summary>
        public Technology Technology { get; set; }


        /// <summary>
        /// Gets or sets assessment date
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyy HH:mm}")]
        [Display(Name = "Date")]
        public DateTime? ExamDate { get; set; }

        /// <summary>
        /// Gets or sets achieved exam score
        /// </summary>
        [Range(0, 100)]
        public int? ExamScore { get; set; }

        /// <summary>
        /// Gets or sets examiner comment
        /// </summary>
        [Display(Name = "Comment")]
        [MaxLength(1000)]
        public string ExamComment { get; set; }

        /// <summary>
        /// Gets or sets indication that assissment was passed/not passed
        /// </summary>
        [Display(Name = "Exam passed")]
        public bool? IsPassed { get; set; }
    }
}
