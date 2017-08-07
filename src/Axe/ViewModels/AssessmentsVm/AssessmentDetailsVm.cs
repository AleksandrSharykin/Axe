using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Axe.Models.AssessmentsVm
{
    /// <summary>
    /// Class provides information to display about assigned <see cref="SkillAssessment"/>
    /// </summary>
    public class AssessmentDetailsVm : SkillAssessment
    {
        /// <summary>
        /// Gets or sets indication that Edit operation is allowed
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Gets or sets indication that Mark operation is allowed
        /// </summary>
        public bool CanMark { get; set; }

        /// <summary>
        /// Gets or sets indication that Delete operation is allowed
        /// </summary>
        public bool CanDelete { get; set; }
    }
}
