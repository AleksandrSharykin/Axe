using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    /// <summary>
    /// Class represents code block which is used in Roslyn platform compiler
    /// </summary>
    public class CodeBlock
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Source code
        /// </summary>
        public string Code { get; set; }

        public string Task { get; set; }

        /// <summary>
        /// Returned result of source code 
        /// </summary>
        public string Output { get; set; }
    }
}
