using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models.ProfileViewModels
{
    /// <summary>
    /// Class contains data which should be displayed in user personal profile
    /// </summary>
    public class ProfileInfoVm: IndexVm
    {
        /// <summary>
        /// Gets or sets indicator that user is viewing their own profile
        /// </summary>
        public bool Self { get; set; }

        /// <summary>
        /// Gets or sets all technologies visible to user
        /// </summary>
        public IEnumerable<Technology> Technologies { get; set; }

        /// <summary>
        /// Gets or sets technology selected by user in a list 
        /// </summary>
        public Technology SelectedTechnology { get; set; }

        /// <summary>
        /// Gets or sets a list of skill assessment taken by user
        /// </summary>
        public IEnumerable<SkillAssessment> Assessments { get; set; }

        /// <summary>
        /// Gets or sets list of exams which user can take in selected technology
        /// </summary>
        public IEnumerable <ExamTask> Tasks { get; set; }

        /// <summary>
        /// Gets or sets all user exam attemps in a selected technology
        /// </summary>
        public IEnumerable<ExamAttempt> AllAttempts { get; set; }

        /// <summary>
        /// Gets or sets best user exam attemps in a selected technology
        /// </summary>
        public IEnumerable<ExamAttempt> BestAttempts { get; set; }
    }
}
