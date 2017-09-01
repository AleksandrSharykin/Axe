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

        /// <summary>
        /// Returns a list of available quizes 
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var request = await this.CreateRequest(0);
            ViewData["UserId"] = request.CurrentUser.Id;
            var list = this.context.RealtimeQuiz
                            .Include(q => q.Judge)
                            .ToList();
            return View(list);
        }

        /// <summary>
        /// Open a page for a judge to ask questions
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Ask(int id)
        {
            var request = await this.CreateRequest(id);

            var quiz = this.context.RealtimeQuiz
                            .Include(q => q.Judge)
                            .Include(q => q.Participants).ThenInclude(x => x.User)
                            .First(q => q.Id == id);

            if (quiz.JudgeId != request.CurrentUser.Id)
            {
                return RedirectToAction(nameof(Answer), new { id });
            }

            ViewData["UserId"] = request.CurrentUser.Id;
            return View(quiz);
        }

        /// <summary>
        /// Open page for participant to answer questions
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Accepts WebSocket commucation requests
        /// </summary>
        /// <returns></returns>
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


        private string GetChatKey(QuizMessage msg)
        {
            return GetChatKey(msg.UserId, msg.QuizId);
        }

        private string GetChatKey(string uid, int quizId)
        {
            return String.Format("{0}+{1}", uid, quizId);
        }

        private WebSocket TryGetChat(string uid, int quizId)
        {
            var key = GetChatKey(uid, quizId);
            WebSocket socket;
            sockets.TryGetValue(key, out socket);
            return socket;
        }

        private bool IsChatBelongsToUser(string chatKey, string uid)
        {
            return chatKey.StartsWith(uid + "+");
        }

        private bool IsChatBelongsToQuiz(string chatKey, int quizId)
        {
            return chatKey.EndsWith("+" + quizId);
        }

        /// <summary>
        /// Marks an answer from participant
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task<IActionResult> Mark(QuizMessage msg)
        {
            var quiz = await this.context.RealtimeQuiz
                                .Include(q => q.Participants).ThenInclude(p => p.User)
                                .FirstAsync(q => q.Id == msg.QuizId);

            // get participant statistics
            var participantId = msg.Text.ToString();
            var entry = quiz.Participants.FirstOrDefault(p => p.UserId == participantId);

            if (entry != null && null == entry.IsEvaluated)
            {
                // mark latest answer and save changes
                bool success = Boolean.TrueString.Equals(msg.Content, StringComparison.OrdinalIgnoreCase);

                if (success)
                {
                    entry.Score++;
                }

                entry.IsEvaluated = success;
                this.context.Update(entry);
                await this.context.SaveChangesAsync();

                // prepare feedback message to judge with updated score
                msg.Content = ToJson(new { UserName = entry.User.UserName, Score = entry.Score });

                WebSocket socket = TryGetChat(participantId, quiz.Id);
                if (socket != null)
                {
                    // send notification to participant about mark result
                    var response = new QuizMessage
                    {
                        QuizId = quiz.Id,
                        UserId = quiz.JudgeId,
                        Content = success.ToString(),
                        Text = entry.Score.ToString(),
                        MessageType = QuizMessageType.Mark,
                    };
                    await SendObject(socket, response);
                }
            }

            return Json(msg);
        }

        /// <summary>
        /// Broadcasts messages to quiz participants
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        private async Task Echo(string uid, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            string chatKey = null;
            while (false == result.CloseStatus.HasValue)
            {
                //  deserialize received message
                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var msg = JsonConvert.DeserializeObject<QuizMessage>(json);

                chatKey = this.GetChatKey(msg);

                var quiz = this.context.RealtimeQuiz.AsNoTracking()
                                .Include(q => q.Participants).ThenInclude(p => p.User)
                                .First(q => q.Id == msg.QuizId);

                if (msg.MessageType == QuizMessageType.Entry)
                {
                    if (msg.UserId != quiz.JudgeId)
                    {
                        // register a new participant
                        QuizParticipant entry = quiz.Participants.FirstOrDefault(p => p.UserId == msg.UserId);
                        if (entry == null)
                        {
                            entry = new QuizParticipant { UserId = msg.UserId, QuizId = quiz.Id };
                            this.context.Add(entry);
                            this.context.SaveChanges();
                        }
                        this.context.Entry(entry).State = EntityState.Detached;

                        // broadcast new entry to judge
                        WebSocket judge = TryGetChat(quiz.JudgeId, quiz.Id);
                        if (judge != null)
                        {
                            msg.Content = ToJson(new { UserName = entry.User.UserName, Score = entry.Score });
                            await SendObject(judge, msg);
                        }
                    }

                    // add participant socket to chat
                    if (sockets.ContainsKey(chatKey))
                        sockets[chatKey] = webSocket;
                    else
                        sockets.Add(chatKey, webSocket);
                }
                else if (msg.MessageType == QuizMessageType.Answer)
                {
                    // accept answer from participant
                    QuizParticipant entry = quiz.Participants.FirstOrDefault(p => p.UserId == msg.UserId);
                    if (entry == null)
                    {
                        entry = new QuizParticipant { UserId = msg.UserId, QuizId = quiz.Id };
                        entry.LastAnswer = msg.Content;
                        this.context.Add(entry);
                        this.context.SaveChanges();
                    }
                    else if (entry.IsEvaluated == null)
                    {
                        entry.LastAnswer = msg.Content;
                        this.context.Update(entry);
                        this.context.SaveChanges();
                    }
                    this.context.Entry(entry).State = EntityState.Detached;

                    if (entry.IsEvaluated == null)
                    {
                        // broadcast new/updated answer to judge
                        WebSocket judge = TryGetChat(quiz.JudgeId, quiz.Id);
                        if (judge != null)
                        {
                            var user = this.context.Users.Single(u => u.Id == msg.UserId);
                            msg.Content = ToJson(new { UserName = user.UserName.ToString(), Answer = msg.Content });
                            await SendObject(judge, msg);
                        }
                    }
                }
                else if (msg.MessageType == QuizMessageType.Question)
                {
                    if (quiz.JudgeId == msg.UserId)
                    {
                        quiz.LastQuestion = msg.Content;
                        foreach (var p in quiz.Participants)
                        {
                            p.IsEvaluated = null;
                            p.LastAnswer = null;
                        }
                        this.context.Update(quiz);
                        this.context.SaveChanges();

                        this.context.Entry(quiz).State = EntityState.Detached;
                        foreach (var p in quiz.Participants)
                        {
                            this.context.Entry(p).State = EntityState.Detached;
                        }
                    }

                    // broadcast new Question to all participants
                    foreach (var s in sockets)
                    {
                        if (IsChatBelongsToUser(s.Key, quiz.JudgeId))
                            continue;

                        if (false == IsChatBelongsToQuiz(s.Key, quiz.Id))
                            continue;
                        await SendResult(s.Value, buffer, result);
                    }
                }
                else
                {
                    foreach (var s in sockets)
                    {
                        if (false == IsChatBelongsToQuiz(s.Key, quiz.Id))
                            continue;
                        await SendResult(s.Value, buffer, result);
                    }
                }

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            if (chatKey != null)
            {
                sockets.Remove(chatKey);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        /// <summary>
        /// Shortcut for json serialization
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>Returns json representation of object</returns>
        private string ToJson(object dto)
        {
            return JsonConvert.SerializeObject(dto, options);
        }

        /// <summary>
        /// Sends object via WebSocket in json format
        /// </summary>
        /// <param name="socket">Socket</param>
        /// <param name="dto">Object</param>
        /// <returns></returns>
        private async Task SendObject(WebSocket socket, object dto)
        {
            var bytes = Encoding.UTF8.GetBytes(ToJson(dto));
            await socket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        /// <summary>
        /// Sends bytes via WebSocket
        /// </summary>
        /// <param name="socket">Socket</param>
        /// <param name="buffer">Bytes array</param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task SendResult(WebSocket socket, byte[] buffer, WebSocketReceiveResult result)
        {
            await socket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
        }
    }
}