using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    /// <summary>
    /// Class contains details about user attempt to pass <see cref="ExamTask"/>
    /// </summary>
    public class ExamAttempt
    {
        /// <summary>
        /// Gets or sets identifier
        /// </summary>
        public int Id { get; set;}


        /// <summary>
        /// Gets or sets <see cref="Technology"/>
        /// </summary>
        public int? TechnologyId { get; set; }

        /// <summary>
        /// Gets or sets <see cref="Technology"/>
        /// </summary>
        public Technology Technology { get; set; }


        /// <summary>
        /// Gets or sets base <see cref="ExamTask"/>
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// Gets or sets base <see cref="ExamTask"/>
        /// </summary>
        public ExamTask Task { get; set; }


        /// <summary>
        /// Gets or sets user taking a test
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Gets or sets user taking a test
        /// </summary>
        public ApplicationUser Student { get; set; }


        /// <summary>
        /// Gets or sets list of question to answer
        /// </summary>
        public IList<AttemptQuestion> Questions { get; set; }

        /// <summary>
        /// Gets or sets maximum possible test score 
        /// </summary>
        public int? MaxScore { get; set; }

        /// <summary>
        /// Gets or sets attempt date
        /// </summary>
        public DateTime? ExamDate { get; set; }

        /// <summary>
        /// Gets or sets achieved score
        /// </summary>
        public int? ExamScore { get; set; }

        /// <summary>
        /// Get or sets indication that test is passed/not passed
        /// </summary>
        public bool? IsPassed { get; set; }                
    }
}
