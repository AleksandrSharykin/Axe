using Axe.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.ViewModels.CompilerVm
{
    public class CodeBlockResultVm
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public CodeBlockResult TypeResult { get; set; }

        public string[] Content { get; set; }
    }
}
