using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    public class TestCaseCodeBlock
    {
        /// <summary>
        /// Gets or sets test case identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets input parameters
        /// </summary>
        [Required]
        public string Input { get; set; }

        /// <summary>
        /// Gets or sets output parameters
        /// </summary>
        [Required]
        public string Output { get; set; }
    }
}
