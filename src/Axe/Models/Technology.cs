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
        /// <summary>
        /// Gets ort sets identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets technology name
        /// </summary>
        [Required]
        [MaxLength(1024)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets short description of a technology
        /// </summary>
        [MaxLength(1024)]
        [Required]
        public string InformationText { get; set; }

        /// <summary>
        /// Gets or sets list of experts
        /// </summary>
        public ICollection<ExpertTechnologyLink> Experts { get; set; }

        /// <summary>
        /// Gets or sets list of exam attempts made by users in this technology
        /// </summary>
        public ICollection<ExamAttempt> Attempts { get; set; }

        /// <summary>
        /// Gets or sets list of skill assessments in this technology
        /// </summary>
        public ICollection<SkillAssessment> Assessments { get; set; }

        /// <summary>
        /// Gets or sets template for code block
        /// </summary>
        public string Template { get; set; }
    }
}
