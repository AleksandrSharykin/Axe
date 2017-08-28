using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Dto
{
    /// <summary>
    /// Class contains data about exam attempts in a certain day
    /// </summary>
    public class StatsExamAttempts
    {
        /// <summary>
        /// Gets or sets date
        /// </summary>
        public string Day { get; set; }

        /// <summary>
        /// Gets or sets count of exam attempts
        /// </summary>
        public int Count { get; set; }
    }
}
