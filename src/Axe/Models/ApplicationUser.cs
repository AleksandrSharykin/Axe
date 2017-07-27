using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Axe.Models
{
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Gets or sets assessments taken by user
        /// </summary>
        public ICollection<SkillAssessment> AssessmentsAsStudent  { get; set; }

        /// <summary>
        /// Gets or sets assessments supervised by by user
        /// </summary>
        public ICollection<SkillAssessment> AssessmentsAsExaminer { get; set; }

        /// <summary>
        /// Gets or sets question created by user
        /// </summary>
        public ICollection<TaskQuestion> Questions { get; set; }

        /// <summary>
        /// Gets or sets exams attempt taken by user
        /// </summary>
        public ICollection<ExamAttempt> Attempts { get; set; }
    }
}
