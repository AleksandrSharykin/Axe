using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    /// <summary>
    /// Class contains data about quiz
    /// </summary>
    public class RealtimeQuiz
    {
        /// <summary>
        /// Gets or sets identifier
        /// </summary>        
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets title
        /// </summary>
        [Required]
        public string Title { get; set; }


        /// <summary>
        /// Gets or sets judge identifier
        /// </summary>
        public string JudgeId { get; set; }

        /// <summary>
        /// Gets or sets judge
        /// </summary>
        public ApplicationUser Judge { get; set; }


        /// <summary>
        /// Gets or sets last asked question
        /// </summary>
        public string LastQuestion { get; set; }

        /// <summary>
        /// Gets or sets list of participants
        /// </summary>
        public ICollection<QuizParticipant> Participants { get; set; }
    }
}
