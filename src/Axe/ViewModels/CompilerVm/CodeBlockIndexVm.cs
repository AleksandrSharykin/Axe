using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Axe.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Axe.ViewModels.CompilerVm
{
    public class CodeBlockIndexVm
    {
        public List<CodeBlockVm> ListOfCodeBlocks { get; set; } = new List<CodeBlockVm>();

        public SelectList ListOfTechnologies { get; set; }

        [Display(Name = "Select technology")]
        public int SelectedTechnologyId { get; set; }
    }
}
