using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Dto
{
    /// <summary>
    /// Class contains data about exams in one technology mark by expert
    /// </summary>
    public class StatsExaminer
    {
        /// <summary>
        /// Gets or sets user identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets user name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets technology name
        /// </summary>
        public string Tech { get; set; }

        /// <summary>
        /// Gets or set number passed assessments marked byexaminer
        /// </summary>
        public int Successful { get; set; }

        /// <summary>
        /// Gets or set number failed assessments marked byexaminer
        /// </summary>
        public int Failed { get; set; }
    }
}
