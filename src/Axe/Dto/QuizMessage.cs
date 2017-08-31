using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Dto
{
    /// <summary>
    /// Class represents format of communication between quiz participants
    /// </summary>
    public class QuizMessage
    {
        /// <summary>
        /// Gets or sets sender identifier
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets quiz identifier
        /// </summary>
        public int QuizId { get; set; }

        /// <summary>
        /// Gets or sets message type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public QuizMessageType MessageType { get; set; }

        /// <summary>
        /// Gets or sets message content
        /// </summary>
        public string Content { get; set; }

        public string Text { get; set; }
    }
}
