using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Axe.Models
{
    /// <summary>
    /// Class contains contains details about user attempt to answer <see cref="TaskQuestion"/>
    /// </summary>
    public class AttemptQuestion
    {
        /// <summary>
        /// Get or sets identifier
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets attempt details
        /// </summary>
        public int AttemptId { get; set; }

        /// <summary>
        /// Gets or sets attempt details
        /// </summary>
        public ExamAttempt Attempt { get; set; }


        /// <summary>
        /// Gets or sets question position in question set 
        /// </summary>
        // todo shuffle task questions for each attempt
        public int SortNumber { get; set; }


        /// <summary>
        /// Gets or sets base <see cref="TaskQuestion"/>
        /// </summary>
        public int? TaskQuestionId { get; set; }

        /// <summary>
        /// Gets or sets base <see cref="TaskQuestion"/>
        /// </summary>
        public TaskQuestion TaskQuestion { get; set; }


        /// <summary>
        /// Gets or setws user answers
        /// </summary>
        public IList<AttemptAnswer> AttemptAnswers { get; set; }
    }
}
