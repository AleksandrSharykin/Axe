using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Dto
{
    /// <summary>
    /// Class contains detailed information about <see cref="Axe.Models.SkillAssessment"/>
    /// </summary>
    public class AssessmentDetails
    {
        /// <summary>
        /// Gets or sets examiner name
        /// </summary>
        public string ExaminerName { get; set; }

        /// <summary>
        /// Gets or sets technology name
        /// </summary>
        public string TechnologyName { get; set; }

        /// <summary>
        /// Gets or sets exam date
        /// </summary>
        public string ExamDate { get; set; }

        /// <summary>
        /// Gets or sets exam score
        /// </summary>
        public int? ExamScore { get; set; }

        /// <summary>
        /// Gets or sets exam comment
        /// </summary>
        public string ExamComment { get; set; }

        /// <summary>
        /// Gets or sets indication that assessment was marked 
        /// </summary>
        public bool? IsPassed { get; set; }

        /// <summary>
        /// Gets or sets indication that Edit operation is allowed
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Gets or sets that Mark operation is allowed
        /// </summary>
        public bool CanMark { get; set; }

        /// <summary>
        /// Gets or sets that Delete operation is allowed
        /// </summary>
        public bool CanDelete { get; set; }
    }
}
