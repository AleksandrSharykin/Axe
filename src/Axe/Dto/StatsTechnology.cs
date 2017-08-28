using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Dto
{
    /// <summary>
    /// Class contains data about average scores in a certain technology
    /// </summary>
    public class StatsTechnology
    {
        /// <summary>
        /// Gets or sets technology identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets technology name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets total exam attempts count
        /// </summary>
        public int AttemptsCount { get; set; }

        /// <summary>
        /// Gets or sets average attempt completion in %
        /// </summary>
        public string AvgAttemptScore { get; set; }

        /// <summary>
        /// Gets or sets total skill assessment count 
        /// </summary>
        public int AssessmentsCount { get; set; }

        /// <summary>
        /// Gets or sets average assessment score
        /// </summary>
        public string AvgAssessmentScore { get; set; }
    }
}
