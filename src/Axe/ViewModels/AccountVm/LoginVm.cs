using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models.AccountVm
{
    /// <summary>
    /// Class provides values required for login
    /// </summary>
    public class LoginVm : AccountBaseVm
    {
        /// <summary>
        /// Gtes or sets password for login
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets indiction to remember user login
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
