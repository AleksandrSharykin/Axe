using Axe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.ViewModels.CompilerVm
{
    public class CodeBlockObserveVm
    {
        public ApplicationUser User { get; set; }

        public CodeBlockVm CodeBlock { get; set; }

        public ApplicationUser Observer { get; set; }
    }
}
