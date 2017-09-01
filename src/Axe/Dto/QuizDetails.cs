using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Dto
{
    /// <summary>
    /// Class contains information about quiz
    /// </summary>
    public class QuizDetails
    {
        /// <summary>
        /// Gets or sets quiz title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Get or sets judge name
        /// </summary>
        public string Judge { get; set; }

        /// <summary>
        /// Gets or sets list of participants wiht their scores
        /// </summary>
        public Dictionary<string, int> Scores { get; set; }
    }
}
