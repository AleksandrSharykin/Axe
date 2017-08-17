using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    /// <summary>
    /// Enumeration of possible question types
    /// </summary>
    public enum TaskQuestionType
    {
        /// <summary>
        /// Multiple selected answer options
        /// </summary>
        MultiChoice,

        /// <summary>
        /// Single selected answer option
        /// </summary>
        SingleChoice,

        /// <summary>
        /// No options for selection; answers should be provided by student
        /// </summary>
        MultiLine,

        /// <summary>
        /// No options for selection; a single answer should be provided by student
        /// </summary>
        SingleLine,

        /// <summary>
        /// Multiple answer options should be sorted in correct order
        /// </summary>
        PrioritySelection
    }
}
