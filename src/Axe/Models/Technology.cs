using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Axe.Models
{
    /// <summary>
    /// Class represents a Technology abstraction
    /// </summary>
    public class Technology
    {
        public int Id { get; set; }

        [MaxLength(1024)]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets short description of a technology
        /// </summary>
        [MaxLength(1024)]
        [Required]
        public string InformationText { get; set; }

        public ICollection<ExpertTechnologyLink> Experts { get; set; }
    }
}
