using Axe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.ViewModels.CompilerVm
{
    public class CodeBlockVm
    {
        public int Id { get; set; }

        public string Task { get; set; }

        public Technology Technology { get; set; }
    }
}
