using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models.TechnologiesVm
{
    /// <summary>
    /// Class provides data requred to display and select experts in <see cref="Technology"/>
    /// </summary>
    public class ExpertSelectionVm
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
        /// Gets or sets user email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets indication that user is selected as expert
        /// </summary>
        public bool IsExpert { get; set; }
    }
}
