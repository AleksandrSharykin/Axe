using System;
using System.Collections.Generic;
using System.Linq;

namespace Axe.Models
{
    public class ExamTask
    {
        public int Id { get; set; }
        
        public string Title { get; set; }
        
        public string Objective { get; set; }

        public int? TechnologyId { get; set; }

        public Technology Technology { get; set; }

        public string AuthorId { get; set; }

        public ApplicationUser Author { get; set; }

        public IList<TaskQuestionLink> Questions { get; set; }

        public bool IsDemonstration { get; set; }
    }
}
