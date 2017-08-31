using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Dto
{
    /// <summary>
    /// Types of quiz messages
    /// </summary>
    public enum QuizMessageType
    {
        /// <summary>
        /// Participant joined quiz
        /// </summary>
        Entry,
        /// <summary>
        /// Judge sent a question
        /// </summary>
        Question,
        /// <summary>
        /// Participant sent an answer
        /// </summary>
        Answer,
        /// <summary>
        /// Judge marked an answer
        /// </summary>
        Mark,
        /// <summary>
        /// Participant left
        /// </summary>
        Exit,
    }
}
