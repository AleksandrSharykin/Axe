using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models.ExamTasksVm
{
    /// <summary>
    /// Class contains quetions data to display for selection in tasks
    /// </summary>
    public class QuestionSelectionVm
    {
        /// <summary>
        /// Gets or sets <see cref="TaskQuestion"/> identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets question text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets a shorten question text preview
        /// </summary>
        public string Preview { get; set; }

        /// <summary>
        /// Gets or sets question type
        /// </summary>
        public TaskQuestionType Type { get; set; }

        /// <summary>
        /// Gets or sets selection indicator
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
