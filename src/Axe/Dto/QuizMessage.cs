using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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

        [JsonConverter(typeof(StringEnumConverter))]
        public QuizMessageType MessageType { get; set; }

        public object Content { get; set; }

        public string Text { get; set; }
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
