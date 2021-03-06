﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace Axe.Models
{
    /// <summary>
    /// Class contains question details with a set of answers
    /// </summary>
    public class TaskQuestion
    {
        /// <summary>
        /// Gets or sets identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets question <see cref="Technology"/>
        /// </summary>
        public int? TechnologyId { get; set; }

        /// <summary>
        /// Gets or sets question <see cref="Technology"/>
        /// </summary>
        public Technology Technology { get; set; }
        
        /// <summary>
        /// Gets or sets question text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets a shorten question text preview
        /// </summary>
        [NotMapped]
        public string Preview
        {
            get
            {
                return Text != null && Text.Length > 128 ? Text.Substring(0, 128) + "..." : Text;
            }
        }

        /// <summary>
        /// Gets or sets question type
        /// </summary>        
        public TaskQuestionType Type { get; set; }

        /// <summary>
        /// Gets indication that user can input values for answers
        /// </summary>
        [NotMapped]
        public bool WithUserInput
        {
            get
            {
                return Type == TaskQuestionType.MultiLine || Type == TaskQuestionType.SingleLine;
            }
        }

        /// <summary>
        /// Gets or sets user who created question
        /// </summary>
        public string AuthorId { get; set; }

        /// <summary>
        /// Gets or sets user who created question
        /// </summary>
        public ApplicationUser Author { get; set; }


        /// <summary>
        /// Gets or sets question answer
        /// </summary>
        public IList<TaskAnswer> Answers { get; set; }

        /// <summary>
        /// Gets or sets list of task where question is used
        /// </summary>
        public ICollection<TaskQuestionLink> Tasks { get; set; }
    }
}
