using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Dto.Compiler
{
    /// <summary>
    /// Represents message for compiler's web-socket
    /// </summary>
    public class CompilerMessage
    {
        /// <summary>
        /// Gets or sets text content
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets identifier of observerd user
        /// </summary>
        public string ObservedUserId { get; set; }

        /// <summary>
        /// Gets or sets identifier of sender
        /// </summary>
        public string SenderUserId { get; set; }

        /// <summary>
        /// Gets or sets identifier of task
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// Gets or sets message type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public CompilerMessageType Type { get; set; }
    }

    public enum CompilerMessageType
    {
        StartedSolveTask,
        FinishedSolveTask,
        MadeChanges,
        StartedObserve,
        FinishedObserve,
        Sync
    }
}
