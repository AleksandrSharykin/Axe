using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    /// <summary>
    /// Class creates many-to-many relationship between <see cref="ExamTask"/> and <see cref="TaskQuestion"/> entities
    /// </summary>
    public class TaskQuestionLink
    {
        /// <summary>
        /// Gets or sets task identifier
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// Gets or sets task link
        /// </summary>
        public ExamTask Task { get; set; }


        /// <summary>
        /// Get or sets question identifier
        /// </summary>
        public int QuestionId { get; set; }

        /// <summary>
        /// Gets or sets question link
        /// </summary>
        public TaskQuestion Question { get; set; }
    }
}
