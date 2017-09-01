using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Axe.Models;
using Axe.Dto;

namespace Axe.Managers
{
    public class QuizManager : ManagerBase, IQuizManager
    {
        private static Dictionary<string, WebSocket> sockets = new Dictionary<string, WebSocket>();

        private JsonSerializerSettings options = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        public QuizManager(AxeDbContext context) : base(context)
        {
        }

        #region Helper methods
        /// <summary>
        /// Gets socket associated with a quiz message
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private string GetChatKey(QuizMessage msg)
        {
            return GetChatKey(msg.UserId, msg.QuizId);
        }

        /// <summary>
        /// Gets socket key for certain participant in a specified quiz
        /// </summary>
        /// <param name="uid">Participant identifier</param>
        /// <param name="quizId">Quiz identifier</param>
        /// <returns></returns>
        private string GetChatKey(string uid, int quizId)
        {
            return String.Format("{0}+{1}", uid, quizId);
        }

        /// <summary>
        /// Get socket for certain participant in a specified quiz
        /// </summary>
        /// <param name="uid">Participant identifier</param>
        /// <param name="quizId">Quiz identifier</param>
        /// <returns></returns>
        private WebSocket TryGetChat(string uid, int quizId)
        {
            var key = GetChatKey(uid, quizId);
            WebSocket socket;
            sockets.TryGetValue(key, out socket);
            return socket;
        }

        /// <summary>
        /// Checks if specified chat key belongs to a certain participant
        /// </summary>
        /// <param name="chatKey">Chat key</param>
        /// <param name="uid">Participant identifier</param>
        /// <returns></returns>
        private bool IsChatBelongsToUser(string chatKey, string uid)
        {
            return chatKey.StartsWith(uid + "+");
        }

        /// <summary>
        /// Checks if specified chat key belongs is associated with a certain participant
        /// </summary>
        /// <param name="chatKey">Chat key</param>
        /// <param name="quizId">Quiz identifier</param>
        private bool IsChatBelongsToQuiz(string chatKey, int quizId)
        {
            return chatKey.EndsWith("+" + quizId);
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
        #endregion

        /// <summary>
        /// Accepts WebSocket communication request, broadcasts messages to quiz participants
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        public async Task Connect(Request<WebSocket> request)
        {
            string uid = request.CurrentUser.Id;
            var webSocket = request.Item;

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
        /// Marks an answer from participant
        /// </summary>
        public async Task<Response<QuizMessage>> Mark(Request<QuizMessage> request)
        {
            var msg = request.Item;

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

            return this.Response(msg);
        }

        /// <summary>
        /// Returns a list of available quizes
        /// </summary>
        public async Task<Response<List<RealtimeQuiz>>> Index()
        {
            var items = await this.context.RealtimeQuiz
                                .Include(q => q.Judge)
                                .ToListAsync();
            return this.Response(items);
        }

        /// <summary>
        /// Returns quiz instance for edit
        /// </summary>
        public async Task<Response<RealtimeQuiz>> InputGet(Request<int?> request)
        {
            var id = request.Item;
            var quiz = await this.context.RealtimeQuiz
                                 .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null || quiz.JudgeId != request.CurrentUser.Id)
            {
                quiz = new RealtimeQuiz();
            }

            return this.Response(quiz);
        }

        /// <summary>
        /// Validates and saves quiz edits
        /// </summary>
        public async Task<Response<RealtimeQuiz>> InputPost(Request<RealtimeQuiz> request)
        {
            RealtimeQuiz quizInput = request.Item;
            int id = quizInput.Id;
            RealtimeQuiz quiz = null;

            if (quizInput.Id > 0)
            {
                quiz = await this.context.RealtimeQuiz
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (quiz == null)
                {
                    return this.NotFound<RealtimeQuiz>();
                }

                if (quiz.JudgeId != request.CurrentUser.Id)
                {
                    request.ModelState.AddModelError(String.Empty, ValidationMessages.Instance.QuizInput);
                }
            }
            else
            {
                quiz = quizInput;
                quiz.JudgeId = request.CurrentUser.Id;
            }

            if (request.ModelState.IsValid)
            {
                quiz.Title = quizInput.Title;

                if (quiz.Id > 0)
                {
                    this.context.Update(quiz);
                }
                else
                {
                    this.context.Add(quiz);
                }

                this.context.SaveChanges();

                return this.Response(quiz);
            }

            return this.ValidationError(quizInput);
        }

        /// <summary>
        /// Returns information about quiz participants
        /// </summary>
        public async Task<Response<QuizDetails>> Details(Request<int> request)
        {
            int id = request.Item;

            var quiz = await this.context.RealtimeQuiz
                                .Include(q => q.Judge)
                                .Include(q => q.Participants).ThenInclude(x => x.User)
                                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                return this.NotFound<QuizDetails>();
            }

            var data = new QuizDetails
            {
                Title = quiz.Title,
                Judge = quiz.Judge.UserName,
                Scores = quiz.Participants.ToDictionary(p => p.User.UserName, p => p.Score),
                CanEdit = quiz.JudgeId == request.CurrentUser.Id
            };

            return this.Response(data);
        }


        /// <summary>
        /// Gets quiz data for asking questions
        /// </summary>
        public async Task<Response<RealtimeQuiz>> Ask(Request<int> request)
        {
            int id = request.Item;

            var quiz = await this.context.RealtimeQuiz
                                .Include(q => q.Judge)
                                .Include(q => q.Participants).ThenInclude(x => x.User)
                                .FirstAsync(q => q.Id == id);

            if (quiz.JudgeId != request.CurrentUser.Id)
            {
                return this.ValidationError(new RealtimeQuiz { Id = id });
            }

            return this.Response(quiz);
        }

        /// <summary>
        /// Gets quiz data for answering
        /// </summary>
        public async Task<Response<RealtimeQuiz>> Answer(Request<int> request)
        {
            int id = request.Item;

            var quiz = await this.context.RealtimeQuiz
                .Include(q => q.Judge)
                .Include(q => q.Participants)
                .FirstAsync(q => q.Id == id);

            quiz.Participants = quiz.Participants.Where(p => p.UserId == request.CurrentUser.Id).Take(1).ToList();

            return this.Response(quiz);
        }
    }
}
