using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Axe.Models
{
    /// <summary>
    /// Class contains answer details for task question 
    /// </summary>
    public class TaskAnswer
    {
        /// <summary>
        /// Gets or sets answer identifier
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets associated question
        /// </summary>
        public int? QuestionId { get; set; }

        /// <summary>
        /// Gets or sets associated question
        /// </summary>
        public TaskQuestion Question { get; set; }

        /// <summary>
        /// Gets or sets answer text
        /// </summary>
        [Display(Prompt = "answer text")]
        [Required]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets correct answer value
        /// </summary>
        [Required]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets points awarded for correct answer
        /// </summary>
        [Display(Prompt = "answer score")]
        [Range(0,100)]
        public int Score { get; set; }

        [NotMapped]
        public bool IsCorrect
        {
            get
            {
                bool b;
                return bool.TryParse(Value, out b) && b;
            }
            set { Value = value.ToString(); }
        }
    }
}
