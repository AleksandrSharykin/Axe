using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Axe.Dto.Compiler
{
    /// <summary>
    /// Represents wrapper for standard object WebSocket
    /// </summary>
    public class WebSocketCompilerWrapper
    {
        /// <summary>
        /// Gets web-socket
        /// </summary>
        public WebSocket Socket { get; }

        public WebSocketCompilerWrapper(WebSocket webSocket)
        {
            this.Socket = webSocket;
        }

        /// <summary>
        /// Identifier task
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// Identifier connected user
        /// </summary>
        public string ConnectedUserId { get; set; }
    }
}
