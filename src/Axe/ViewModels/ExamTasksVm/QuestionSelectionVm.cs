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
        public string Preview
        {
            get
            {
                return Text != null && Text.Length > 128 ? Text.Substring(0, 128) + "..." : Text;
            }
        }

        /// <summary>
        /// Gets or sets selection indicator
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
