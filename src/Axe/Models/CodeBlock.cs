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
        public OutputTypeEnum OutputType { get; set; }

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
    public enum OutputTypeEnum
    {
        [Display(Name = "int")]
        INT,
        [Display(Name = "double")]
        DOUBLE,
        [Display(Name = "string")]
        STRING,
        [Display(Name = "int[]")]
        INT_ARRAY,
        [Display(Name = "double[]")]
        DOUBLE_ARRAY,
        [Display(Name = "string[]")]
        STRING_ARRAY
    }

    /// <summary>
    /// Enum represents result of execution code block
    /// </summary>
    public enum CodeBlockResult
    {
        SUCCESS,
        FAILED,
        ERROR
    }
}
