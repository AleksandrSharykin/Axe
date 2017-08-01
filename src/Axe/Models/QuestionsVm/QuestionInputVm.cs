using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Axe.Models.QuestionsVm
{
    public class QuestionInputVm
    {
        public int Id { get; set; }        

        public SelectList Technologies { get; set; }

        [Display(Name = "Technology")]
        [Required]
        public int? TechnologyId { get; set; }

        [Display(Prompt = "Ask me something")]
        [Required]
        public string Text { get; set; }

        public IList<TaskAnswer> Answers { get; set; }
    }
}
