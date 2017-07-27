using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    public class ExamTask
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Objective { get; set; }

        public int TechnologyId { get; set; }

        public Technology Technology { get; set; }

        public ICollection<TaskQuestion> Questions { get; set; }
    }
}
