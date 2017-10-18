using Axe.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.ViewModels.CompilerVm
{
    public class CodeBlockSolveVm
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

        [Display(Name = "Technology")]
        public Technology Technology { get; set; }

        public int SelectedTechnologyId { get; set; }

        public SelectList ListOfTechnologies { get; set; }

        public DateTime DateLastChanges { get; set; } = DateTime.MinValue;

        public bool isMonitoringMode { get; set; } = false;
    }
}
