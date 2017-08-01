using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Axe.Models.AssessmentsVm
{
    public class AssessmentDetailsVm : SkillAssessment
    {
        public bool CanDelete { get; set; }

        public bool CanMark { get; set; }

        public bool CanEdit { get; set; }
    }
}
