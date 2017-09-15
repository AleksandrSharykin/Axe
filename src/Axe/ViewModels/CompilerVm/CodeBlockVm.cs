using Axe.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.ViewModels.CompilerVm
{
    public class CodeBlockVm
    {
        /// <summary>
        /// Gets or sets code block identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets template code
        /// </summary>
        [Display(Name = "Source code")]
        [Required(ErrorMessage = "Source code is required")]
        public string SourceCode { get; set; }

        /// <summary>
        /// Gets or sets task
        /// </summary>
        [Required]
        public string Task { get; set; }
    }
}
