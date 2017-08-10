using System;
using System.Collections.Generic;
using System.Linq;

namespace Axe.Models
{
    /// <summary>
    /// Class contains a set of questions with asnwers to test knowlendge in a certain <see cref="Technology"/>
    /// </summary>
    public class ExamTask
    {
        /// <summary>
        /// Gets or sets task identifier
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets task title
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Gets or sets task objectives
        /// </summary>
        public string Objective { get; set; }

        /// <summary>
        /// Gets or sets indiction of a demonstration task accessible for anonymous users
        /// </summary>
        public bool IsDemonstration { get; set; }


        /// <summary>
        /// Gets or sets <see cref="Technology"/>
        /// </summary>
        public int? TechnologyId { get; set; }

        /// <summary>
        /// Gets or sets <see cref="Technology"/>
        /// </summary>
        public Technology Technology { get; set; }


        /// <summary>
        /// Gets or sets user who created task
        /// </summary>
        public string AuthorId { get; set; }

        /// <summary>
        /// Gets or sets user who created task
        /// </summary>
        public ApplicationUser Author { get; set; }


        /// <summary>
        /// Gets or sets questions included in task
        /// </summary>
        public ICollection<TaskQuestionLink> Questions { get; set; }
    }
}
