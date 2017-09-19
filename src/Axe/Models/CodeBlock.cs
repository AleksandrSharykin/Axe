using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        /// Gets or sets task identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets verification code
        /// </summary>
        [Required]
        public string VerificationCode { get; set; }

        /// <summary>
        /// Gets or sets type of output
        /// </summary>
        [Required]
        public SupportedType OutputType { get; set; }

        /// <summary>
        /// Gets or sets list of test cases
        /// </summary>
        [Required]
        public List<TestCaseCodeBlock> TestCases { get; set; } = new List<TestCaseCodeBlock>();

        /// <summary>
        /// Gets or sets task
        /// </summary>
        [Required]
        public string Task { get; set; }
    }

    /// <summary>
    /// Enum represents type of output
    /// </summary>
    public enum SupportedType
    {
        [Display(Name = "int")]
        Int,
        [Display(Name = "double")]
        Double,
        [Display(Name = "string")]
        String,
        [Display(Name = "int[]")]
        IntArray,
        [Display(Name = "double[]")]
        DoubleArray,
        [Display(Name = "string[]")]
        StringArray
    }

    /// <summary>
    /// Enum represents result of execution code block
    /// </summary>
    public enum CodeBlockResult
    {
        Success,
        Failed,
        Error,
        Unknown
    }
}
