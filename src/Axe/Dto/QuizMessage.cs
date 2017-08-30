using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Dto
{
    public class QuizMessage
    {
        public string UserId { get; set; }

        public int QuizId { get; set; }

        public QuizMessageType MessageType { get; set; }

        public string Content { get; set; }
    }

    public enum QuizMessageType
    {
        Entry,
        Question,
        Answer,
        Score,
        Exit,
    }
}
