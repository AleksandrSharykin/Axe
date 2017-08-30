using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    public class QuizParticipant
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int QuizId { get; set; }
        public RealtimeQuiz Quiz { get; set; }

        public int Score { get; set; }

        public string LastAnswer { get; set; }

        public bool IsEvaluated { get; set; }
    }
}
