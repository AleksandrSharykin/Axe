using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    /// <summary>
    /// Class creates many-to-many relationship between <see cref="RealtimeQuiz"/> and <see cref="ApplicationUser"/> entities
    /// </summary>
    public class QuizParticipant
    {
        /// <summary>
        /// Gets or sets user identifier
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets user
        /// </summary>
        public ApplicationUser User { get; set; }


        /// <summary>
        /// Gets or sets quiz identifier
        /// </summary>
        public int QuizId { get; set; }

        /// <summary>
        /// Gets or sets quiz
        /// </summary>
        public RealtimeQuiz Quiz { get; set; }


        /// <summary>
        /// Gets or sets participant score
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Gets or sets last participant answer
        /// </summary>
        public string LastAnswer { get; set; }

        /// <summary>
        /// Gets or sets indication that answer was marked by judge
        /// </summary>
        public bool? IsEvaluated { get; set; }
    }
}
