using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Axe.Models.ProfilesVm
{
    /// <summary>
    /// Class contains values to display in Users index
    /// </summary>
    public class IndexVm
    {
        /// <summary>
        /// Gets or sets <see cref="ApplicationUser"/> identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets user name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or set user job description
        /// </summary>
        public string JobPosition { get; set; }

        /// <summary>
        /// Gets or sets user contact info (email)
        /// </summary>
        public string ContactInfo { get; set; }

        /// <summary>
        /// Gets or sets list of <see cref="Technology"/> user is an expert in
        /// </summary>
        public IList<Technology> ExpertKnowledge { get; set; }

        /// <summary>
        /// Gets or sets list of successful and pending <see cref="SkillAssessment"/>s
        /// </summary>
        public IList<SkillAssessment> Skills { get; set; }        
    }
}
