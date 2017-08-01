using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Axe.Models
{
    public class TaskAnswer
    {
        public int Id { get; set; }
        
        public int? QuestionId { get; set; }

        public TaskQuestion Question { get; set; }

        [Display(Prompt = "answer text")]
        [Required]
        public string Text { get; set; }

        [Required]
        public string Value { get; set; }

        [Display(Prompt = "answer score")]
        public int Score { get; set; }

        [NotMapped]
        public bool IsCorrect
        {
            get { return bool.Parse(Value); }
            set { Value = value.ToString(); }
        }
    }
}
