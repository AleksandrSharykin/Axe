using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    public class TaskQuestion
    {
        public int Id { get; set; }

        public int TaskId { get; set; }

        public ExamTask Task { get; set; }

        public string Text { get; set; }

        public int Type { get; set; }        

        public ICollection<TaskAnswer> Answers { get; set; }

        public string AuthorId { get; set; }

        public ApplicationUser Author { get; set; }
    }
}
