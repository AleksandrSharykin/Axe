using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        /// Gets or sets base <see cref="TaskQuestion"/>
        /// </summary>
        public int? TaskQuestionId { get; set; }

        /// <summary>
        /// Gets or sets base <see cref="TaskQuestion"/>
        /// </summary>
        public TaskQuestion TaskQuestion { get; set; }

        /// <summary>
        /// Gets or sets selected answer Id for single choice question
        /// </summary>
        [NotMapped]
        public int? SelectedAnswerId { get; set; }

        /// <summary>
        /// Gets or setws user answers
        /// </summary>
        public IList<AttemptAnswer> AttemptAnswers { get; set; }

        /// <summary>
        /// Gets or sets indication that question answers were evaluated
        /// </summary>
        public bool? IsAccepted { get; set; }

        /// <summary>
        /// Gets or sets indication that all correct answers were provided
        /// </summary>
        public bool? IsPerfect { get; set; }

        /// <summary>
        /// Gets or sets points awarded for answering question
        /// </summary>
        public int? Score { get; set; }

        /// <summary>
        /// Gets or sets question position in exam attempt questions list
        /// </summary>
        public int SortNumber { get; set; }

        /// <summary>
        /// Gets or sets selection options for priority questions
        /// </summary>
        [NotMapped]
        public IList<SelectListItem> PriorityOptions
        {
            get
            {
                if (this.AttemptAnswers != null && this.AttemptAnswers.Count > 0)
                {
                    return Enumerable.Range(1, this.AttemptAnswers.Count)
                                                    .Select(i => new SelectListItem { Text = i.ToString(), Value = i.ToString() })
                                                    .ToList();
                }
                return new List<SelectListItem>();
            }
        }
    }
}
