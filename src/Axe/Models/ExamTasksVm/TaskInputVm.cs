using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Axe.Models.ExamTasksVm
{
    public class TaskInputVm
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Objective { get; set; }

        public int TechnologyId { get; set; }

        public string TechnologyName { get; set; }

        public IList<QuestionSelectionVm> Questions { get; set; }
    }
}
