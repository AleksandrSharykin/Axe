using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    /// <summary>
    /// Class creates many-to-many relationship between <see cref="ApplicationUser"/> and <see cref="Axe.Models.Technology"/> entities
    /// </summary>
    public class ExpertTechnologyLink
    {
        /// <summary>
        /// Gets or sets expert user identifier
        /// </summary>        
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets link to expert user
        /// </summary>
        public ApplicationUser User { get; set; }


        /// <summary>
        /// Gets or sets technology identifier
        /// </summary>
        public int TechnologyId { get; set; }

        /// <summary>
        /// Gets or sets link to technology
        /// </summary>
        public Technology Technology { get; set; }
    }
}
