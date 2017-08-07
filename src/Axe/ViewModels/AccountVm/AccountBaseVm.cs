using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Axe.Models.AccountVm
{
    /// <summary>
    /// Class contains data required for any operation with user account
    /// </summary>
    public class AccountBaseVm
    {
        /// <summary>
        /// Gets or sets user email
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }        
    }
}
