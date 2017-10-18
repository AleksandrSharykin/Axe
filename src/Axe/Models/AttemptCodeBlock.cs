using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    public class AttemptCodeBlock
    {
        /// <summary>
        /// Gets or sets attempt identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets date of last changes
        /// </summary>
        public DateTime DateLastChanges { get; set; }

        /// <summary>
        /// Gets or sets code block
        /// </summary>
        public CodeBlock CodeBlock { get; set; }

        /// <summary>
        /// Gets or sets code block identifier
        /// </summary>
        public int CodeBlockId { get; set; }

        /// <summary>
        /// Gets or sets source code of attempt
        /// </summary>
        public string SourceCode { get; set; }

        /// <summary>
        /// Gets or sets user
        /// </summary>
        public ApplicationUser User { get; set; }

        /// <summary>
        /// Gets or sets user identifier
        /// </summary>
        public string UserId { get; set; }
    }
}
