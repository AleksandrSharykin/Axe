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
using Newtonsoft.Json;
using Axe.Dto;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace Axe.Controllers
{
    [Authorize]
    public class QuizController : ControllerExt
    {
        private static Dictionary<string, WebSocket> sockets = new Dictionary<string, WebSocket>();

        private JsonSerializerSettings options = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        public QuizController(UserManager<ApplicationUser> userManager, AxeDbContext context) : base(userManager, context)
        {
        }

        public async Task<IActionResult> Index()
        {
            var request = await this.CreateRequest(0);
            ViewData["UserId"] = request.CurrentUser.Id;
            var list = this.context.RealtimeQuiz
                            .Include(q => q.Judge)
                            .ToList();
            return View(list);
        }

        public async Task<IActionResult> Ask(int id)
        {
            var request = await this.CreateRequest(id);
            ViewData["UserId"] = request.CurrentUser.Id;

            var quiz = this.context.RealtimeQuiz
                            .Include(q => q.Judge)
                            .Include(q => q.Participants).ThenInclude(x => x.User)
                            .First(q => q.Id == id);
            return View(quiz);
        }

        public async Task<IActionResult> Answer(int id)
        {
            var request = await this.CreateRequest(id);
            ViewData["UserId"] = request.CurrentUser.Id;

            var quiz = this.context.RealtimeQuiz
                            .Include(q => q.Judge)
                            .Include(q => q.Participants)
                            .First(q => q.Id == id);

            quiz.Participants = quiz.Participants.Where(p => p.UserId == request.CurrentUser.Id).Take(1).ToList();
            return View(quiz);
        }

        public async Task<JsonResult> Participate()
        {
            var request = await this.CreateRequest(0);

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                await Echo(request.CurrentUser.Id, webSocket);
            }

            return Json(new { request.CurrentUser.Id });
        }

        public async Task<IActionResult> Mark(QuizMessage msg)
        {
            var quiz = await this.context.RealtimeQuiz
                                .Include(q => q.Participants).ThenInclude(p => p.User)
                                .FirstAsync(q => q.Id == msg.QuizId);

            var participantId = msg.Text.ToString();
            var entry = quiz.Participants.FirstOrDefault(p => p.UserId == participantId);

            if (entry != null && null == entry.IsEvaluated)
            {
                entry.Score++;
                entry.IsEvaluated = true;
                this.context.Update(entry);
                await this.context.SaveChangesAsync();
                msg.Content = new { UserName = entry.User.UserName, Score = entry.Score };

                WebSocket socket;
                if (sockets.TryGetValue(participantId, out socket))
                {
                    var response = new QuizMessage
                    {
                        QuizId = quiz.Id,
                        UserId = quiz.JudgeId,
                        Content = true,
                        Text = entry.Score.ToString(),
                        MessageType = QuizMessageType.Mark,
                    };
                    await SendObject(socket, response);
                }
            }

            return Json(msg);
        }

        RealtimeQuiz _q;
        QuizParticipant _p;


        /// <summary>
        /// Broadcasts quiz messages to all participants
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        private async Task Echo(string uid, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var msg = JsonConvert.DeserializeObject<QuizMessage>(json);

                // bug need to recreate context                
                var quiz = this.context.RealtimeQuiz.AsNoTracking()
                    .Include(q => q.Participants).ThenInclude(p => p.User)
                                .First(q => q.Id == msg.QuizId);

                if (ReferenceEquals(_q, quiz))
                {

                }
                _q = quiz;

                if (msg.MessageType == QuizMessageType.Entry)
                {
                    if (msg.UserId != quiz.JudgeId)
                    {
                        QuizParticipant entry = quiz.Participants.FirstOrDefault(p => p.UserId == msg.UserId);

                        if (ReferenceEquals(_p, entry))
                        {

                        }
                        if (entry == null)
                        {
                            entry = new QuizParticipant { UserId = msg.UserId, QuizId = quiz.Id };
                            _p = entry;
                            this.context.Add(entry);
                            this.context.SaveChanges();
                        }
                        this.context.Entry(entry).State = EntityState.Detached;

                        // broadcast new entry to judge
                        WebSocket judgeSocket;
                        if (sockets.TryGetValue(quiz.JudgeId, out judgeSocket))
                        {
                            msg.Content = new { UserName = entry.User.UserName, Score = entry.Score };
                            await SendObject(judgeSocket, msg);
                        }
                    }
                    if (sockets.ContainsKey(uid))
                        sockets[uid] = webSocket;
                    else
                        sockets.Add(uid, webSocket);
                }
                else if (msg.MessageType == QuizMessageType.Answer)
                {
                    QuizParticipant entry = quiz.Participants.FirstOrDefault(p => p.UserId == msg.UserId);

                    if (ReferenceEquals(_p, entry))
                    {

                    }

                    if (entry == null)
                    {
                        entry = new QuizParticipant { UserId = msg.UserId, QuizId = quiz.Id };
                        this.context.Add(entry);
                        this.context.SaveChanges();
                    }
                    else if (entry.IsEvaluated == null)
                    {
                        entry.LastAnswer = msg.Content.ToString();
                        this.context.Update(entry);
                        this.context.SaveChanges();
                    }
                    this.context.Entry(entry).State = EntityState.Detached;
                    _p = entry;

                    if (entry.IsEvaluated == null)
                    {
                        // broadcast new/updated answer to judge
                        WebSocket judgeSocket;
                        if (sockets.TryGetValue(quiz.JudgeId, out judgeSocket))
                        {
                            msg.Content = new { UserName = msg.UserId.ToString(), Answer = msg.Content.ToString() };
                            await SendObject(judgeSocket, msg);
                        }
                    }
                }
                else if (msg.MessageType == QuizMessageType.Question)
                {
                    if (quiz.JudgeId == msg.UserId)
                    {
                        quiz.LastQuestion = msg.Content.ToString();
                        foreach (var p in quiz.Participants)
                        {
                            p.IsEvaluated = null;
                        }
                        this.context.Update(quiz);
                        this.context.SaveChanges();

                        this.context.Entry(quiz).State = EntityState.Detached;
                        foreach (var p in quiz.Participants)
                        {
                            this.context.Entry(p).State = EntityState.Detached;
                        }
                    }

                    foreach (var s in sockets)
                    {
                        //if (s.Key == id)
                        //    continue;
                        await SendResult(s.Value, buffer, result);
                    }
                }
                else
                {
                    foreach (var s in sockets)
                    {
                        //if (s.Key == id)
                        //    continue;
                        await SendResult(s.Value, buffer, result);
                    }
                }

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            sockets.Remove(uid);
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private async Task SendObject(WebSocket socket, object dto)
        {
            var response = JsonConvert.SerializeObject(dto, options);
            var bytes = Encoding.UTF8.GetBytes(response);
            await socket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task SendResult(WebSocket s, byte[] buffer, WebSocketReceiveResult result)
        {
            await s.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
        }
    }
}