using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Axe.Models.QuestionsVm
{
    /// <summary>
    /// Class contains data required for creation of a new <see cref="TaskQuestion"/>
    /// </summary>
    public class QuestionInputVm
    {
        /// <summary>
        /// Gets or sets <see cref="TaskQuestion"/> identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets question text
        /// </summary>
        [Required]
        [Display(Prompt = "Ask me something")]
        public string Text { get; set; }


        /// <summary>
        /// Gets or sets <see cref="Technology"/> a question belongs to
        /// </summary>
        [Required]
        [Display(Name = "Technology")]
        public int? TechnologyId { get; set; }

        /// <summary>
        /// Gets or sets list of <see cref="Technology"/> for selection
        /// </summary>
        public SelectList Technologies { get; set; }


        /// <summary>
        /// Gets or sets question type
        /// </summary>
        public TaskQuestionType EditorType { get; set; }

        /// <summary>
        /// Gets or sets indiction that user input in asnwers is allowed
        /// </summary>
        public bool WithUserInput { get; set; }

        /// <summary>
        /// Gets or sets correct answer number  for question with single choice
        /// </summary>
        public int? SelectedAnswer { get; set; }        

        /// <summary>
        /// Gets or sets alternative answers
        /// </summary>
        public IList<TaskAnswer> Answers { get; set; }
    }
}
