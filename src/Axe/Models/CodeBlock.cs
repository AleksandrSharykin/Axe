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
        [Display(Name = "bool")]
        Bool,
        [Display(Name = "byte")]
        Byte,
        [Display(Name = "sbyte")]
        Sbyte,
        [Display(Name = "short")]
        Short,
        [Display(Name = "ushort")]
        Ushort,
        [Display(Name = "int")]
        Int,
        [Display(Name = "uint")]
        Uint,
        [Display(Name = "long")]
        Long,
        [Display(Name = "ulong")]
        Ulong,
        [Display(Name = "double")]
        Double,
        [Display(Name = "float")]
        Float,
        [Display(Name = "decimal")]
        Decimal,
        [Display(Name = "char")]
        Char,
        [Display(Name = "string")]
        String,

        [Display(Name = "bool[]")]
        BoolArray,
        [Display(Name = "byte[]")]
        ByteArray,
        [Display(Name = "sbyte[]")]
        SbyteArray,
        [Display(Name = "short[]")]
        ShortArray,
        [Display(Name = "ushort[]")]
        UshortArray,
        [Display(Name = "int[]")]
        IntArray,
        [Display(Name = "uint[]")]
        UintArray,
        [Display(Name = "long[]")]
        LongArray,
        [Display(Name = "ulong[]")]
        UlongArray,
        [Display(Name = "double[]")]
        DoubleArray,
        [Display(Name = "float[]")]
        FloatArray,
        [Display(Name = "decimal[]")]
        DecimalArray,
        [Display(Name = "char[]")]
        CharArray,
        [Display(Name = "string[]")]
        StringArray,
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
