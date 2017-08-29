using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Axe.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.WebSockets;
using System.Threading;

namespace Axe.Controllers
{
    [Authorize]
    public class QuizController : ControllerExt
    {
        private static Dictionary<string, WebSocket> sockets = new Dictionary<string, WebSocket>();

        public QuizController(UserManager<ApplicationUser> userManager, AxeDbContext context) : base(userManager, context)
        {
        }

        public async Task<IActionResult> Index()
        {
            var request = await this.CreateRequest(0);
            ViewData["UserId"] = request.CurrentUser.Id;
            var list = this.context.RealtimeQuiz.Include(q => q.Judge).ToList();
            return View(list);
        }

        public async Task<IActionResult> Ask()
        {
            return View();
        }

        public async Task<IActionResult> Answer()
        {
            return View();
        }

        public async Task<JsonResult> Participate()
        {
            var request = await this.CreateRequest(0);

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                sockets.Add(request.CurrentUser.Id, webSocket);
                await Echo(request.CurrentUser.Id, webSocket);
            }

            return Json(new { request.CurrentUser.Id });
        }

        private async Task Echo(string id, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                foreach (var s in sockets)
                {
                    //if (s.Key == id)
                    //    continue;
                    await s.Value.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                }
                //await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            sockets.Remove(id);
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}