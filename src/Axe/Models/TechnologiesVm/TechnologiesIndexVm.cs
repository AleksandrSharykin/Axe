using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models.TechnologiesVm
{
    public class TechnologiesIndexVm
    {
        public IList<Technology> Technologies { get; set; }

        public Technology SelectedTechnology { get; set; }

        public IList<ExamTask> Exams { get; set; }

        public IList<TaskQuestion> Questions { get; set; }
    }
}
