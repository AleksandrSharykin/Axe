using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    public class TaskAnswer
    {
        public int Id { get; set; }

        public int QuestionId { get; set; }

        public TaskQuestion Question { get; set; }

        public string Text { get; set; }

        public string Value { get; set; }
    }
}
