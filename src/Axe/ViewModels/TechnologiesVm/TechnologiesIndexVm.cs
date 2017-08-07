using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models.TechnologiesVm
{
    /// <summary>
    /// Class contains data to display for selected technology
    /// </summary>
    public class TechnologiesIndexVm
    {
        /// <summary>
        /// Gets or sets a list of technologies for selection
        /// </summary>
        public IList<Technology> Technologies { get; set; }

        /// <summary>
        /// gets or sets selected technology
        /// </summary>
        public Technology SelectedTechnology { get; set; }

        /// <summary>
        /// Gets or sets a list of test tasks in selected technology
        /// </summary>
        public IList<ExamTask> Exams { get; set; }

        /// <summary>
        /// Gets or sets a list of questions in selected technology
        /// </summary>
        public IList<TaskQuestion> Questions { get; set; }

        /// <summary>
        /// Gets or sets a list of Users to assign as experts in selected technology
        /// </summary>
        public IList<ExpertSelectionVm> Experts { get; set; }
    }
}
