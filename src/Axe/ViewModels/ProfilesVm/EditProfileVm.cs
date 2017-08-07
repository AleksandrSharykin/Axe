using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Axe.Models.ProfilesVm
{
    /// <summary>
    /// Class provides values to change user account details
    /// </summary>
    public class EditProfileVm
    {
        [Required]
        [StringLength(255, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Display(Name = "Job Position")]
        public string JobPosition { get; set; }

        // todo add image
    }
}
