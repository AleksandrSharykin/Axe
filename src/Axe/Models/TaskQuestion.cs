using System;
using System.Collections.Generic;
using System.Linq;

namespace Axe.Models
{
    public class TaskQuestion
    {
        public int Id { get; set; }

        public int? TechnologyId { get; set; }

        public Technology Technology { get; set; }
        
        public string Text { get; set; }

        public int Type { get; set; }        

        public ICollection<TaskAnswer> Answers { get; set; }

        public ICollection<TaskQuestionLink> Tasks { get; set; }

        public string AuthorId { get; set; }

        public ApplicationUser Author { get; set; }
    }
}
