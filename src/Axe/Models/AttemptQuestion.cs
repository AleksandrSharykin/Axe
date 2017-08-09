using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public int SortNumber { get; set; }


        /// <summary>
        /// Gets or sets base <see cref="TaskQuestion"/>
        /// </summary>
        public int? TaskQuestionId { get; set; }

        /// <summary>
        /// Gets or sets base <see cref="TaskQuestion"/>
        /// </summary>
        public TaskQuestion TaskQuestion { get; set; }


        [NotMapped]
        public int? SelectedAnswer { get; set; }


        /// <summary>
        /// Gets or setws user answers
        /// </summary>
        public IList<AttemptAnswer> AttemptAnswers { get; set; }

        /// <summary>
        /// Gets or sets indication that question answers were evaluated
        /// </summary>
        public bool? IsAccepted { get; set; }


        /// <summary>
        /// Gets or sets points awarded for answering question
        /// </summary>
        public int? Score { get; set; }
    }
}
