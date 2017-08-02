using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    /// <summary>
    /// Class creates many-to-many relationship between <see cref="ExamTask"/> and <see cref="TaskQuestion"/> entities
    /// </summary>
    public class TaskQuestionLink
    {
        public int? TaskId { get; set; }
        public ExamTask Task { get; set; }

        public int QuestionId { get; set; }
        public TaskQuestion Question { get; set; }
    }
}
