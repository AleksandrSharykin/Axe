using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Dto
{
    /// <summary>
    /// Class contains data about question passing rate
    /// </summary>
    public class StatsQuestion
    {
        /// <summary>
        /// Gets or sets question identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets technology
        /// </summary>
        public string TechnologyName { get; set; }

        /// <summary>
        /// Gets or sets percentage of successful answers
        /// </summary>
        public int Percentage { get; set; }

        /// <summary>
        /// Gets or sets number of successful answers
        /// </summary>
        public int Successful { get; set; }

        /// <summary>
        /// Gets or sets total number of attempts to answer
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Gets or sets question text
        /// </summary>
        public string Preview { get; set; }
    }
}
