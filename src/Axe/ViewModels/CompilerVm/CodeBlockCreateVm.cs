using Axe.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.ViewModels.CompilerVm
{
    public class CodeBlockCreateVm
    {
        /// <summary>
        /// Gets or sets task identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets task
        /// </summary>
        [Required]
        public string Task { get; set; }

        /// <summary>
        /// Gets or sets type of output
        /// </summary>
        [Required]
        [Display(Name = "Type of output")]
        public SupportedType OutputType { get; set; }

        public SelectList ListOfTechnologies { get; set; }

        [Display(Name = "Select technology")]
        public int SelectedTechnologyId { get; set; }

        /// <summary>
        /// Gets or sets list of test cases
        /// </summary>
        [Required]
        [Display(Name = "List of test cases")]
        public List<TestCaseCodeBlock> TestCases { get; set; } = new List<TestCaseCodeBlock>();
    }
}
