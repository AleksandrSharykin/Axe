using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Axe.Models.ExamTasksVm
{
    /// <summary>
    /// Class contains data required for creation of a new test
    /// </summary>
    public class TaskInputVm
    {
        /// <summary>
        /// Gets or sets <see cref="ExamTask"/> identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets test name
        /// </summary>
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets test objectives
        /// </summary>
        [Required]
        public string Objective { get; set; }


        /// <summary>
        /// Gets or sets <see cref="Technology"/>
        /// </summary>
        [Required]
        public int TechnologyId { get; set; }

        /// <summary>
        /// Gets or sets <see cref="Technology"/>
        /// </summary>
        public string TechnologyName { get; set; }


        /// <summary>
        /// Gets or sets list of question in the test
        /// </summary>
        public IList<QuestionSelectionVm> Questions { get; set; }

        /// <summary>
        /// Gets or sets indication thst test is demonstration
        /// </summary>
        [Display(Name = "This is a Demo")]
        public bool IsDemonstration { get; set; }
    }
}
