using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    public class RealtimeQuiz
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string JudgeId { get; set; }

        public ApplicationUser Judge { get; set; }

        public IList<QuizParticipant> Participants { get; set; }
    }
}
