using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models.AccountVm
{
    /// <summary>
    /// Class contains data required for password rest
    /// </summary>
    public class ResetPasswordVm : AccountBaseVm
    {
        /// <summary>
        /// Gets or sets new password
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets password confirmation
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Gets or sets verification code
        /// </summary>
        public string Code { get; set; }
    }
}
