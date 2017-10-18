using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Axe.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Axe.ViewModels.CompilerVm;
using Microsoft.EntityFrameworkCore;
using Axe.Managers;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using System.Threading;
using Newtonsoft.Json;
using System.Text;
using Axe.Dto.Compiler;
using Newtonsoft.Json.Serialization;

namespace Axe.Controllers
{
    public class CompilerController : ControllerExt
    {
        #region FIELDS
        private readonly ICompilerManager compileManager;
        private readonly ITechnologyManager technologyManager;

        /// <summary>
        /// Dictionary of observers
        /// </summary>
        private static Dictionary<string, WebSocketCompilerWrapper> observersWebSockets = new Dictionary<string, WebSocketCompilerWrapper>();
        
        /// <summary>
        /// Dictionary of users which try to solve tasks
        /// </summary>
        private static Dictionary<string, WebSocketCompilerWrapper> usersWebSockets = new Dictionary<string, WebSocketCompilerWrapper>();
        #endregion

        public CompilerController(UserManager<ApplicationUser> userManager,
            ICompilerManager compileManager,
            ITechnologyManager technologyManager,
            AxeDbContext context): base(userManager, context)
        {
            this.compileManager = compileManager;
            this.technologyManager = technologyManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CodeBlockIndexVm model)
        {
            model.ListOfCodeBlocks = await compileManager.GetCodeBlocks<CodeBlockVm>(model.SelectedTechnologyId);
            model.ListOfTechnologies = new SelectList(await technologyManager.GetTechnologies(), "Id", "Name");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Solve(int id)
        {
            try
            {
                var model = await compileManager.GetCodeBlockById<CodeBlockSolveVm>(id);
                var userId = userManager.GetUserId(this.HttpContext.User);
                var attempt = await compileManager.GetAttempt<AttemptCodeBlock>(userId, id);
                if (attempt != null)
                {
                    model.SourceCode = attempt.SourceCode != null ? attempt.SourceCode : "";
                    model.DateLastChanges = attempt.DateLastChanges;
                }
                model.ListOfTechnologies = new SelectList(await technologyManager.GetTechnologies(), "Id", "Name");
                return View(model);
            }
            catch
            {
                return NotFound();
            }
        }

        [Authorize(Roles = "superuser")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CodeBlockCreateVm();
            model.ListOfTechnologies = new SelectList(await technologyManager.GetTechnologies(), "Id", "Name");
            model.TestCases.Add(new TestCaseCodeBlock());
            return View(model);
        }

        [Authorize(Roles = "superuser")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CodeBlockCreateVm model)
        {
            try
            {
                model.ListOfTechnologies = new SelectList(await technologyManager.GetTechnologies(), "Id", "Name");
                if (ModelState.IsValid)
                {
                    await compileManager.Create(model);
                    return RedirectToAction(nameof(CompilerController.Index));
                }
                return View(model);
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("Task", exception.Message);
                return View(model);
            }
        }

        [Authorize(Roles = "superuser")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var model = await compileManager.GetCodeBlockById<CodeBlockCreateVm>(id);
                model.ListOfTechnologies = new SelectList(await technologyManager.GetTechnologies(), "Id", "Name");
                return View(model);
            }
            catch
            {
                return NotFound();
            }
        }

        [Authorize(Roles = "superuser")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CodeBlockCreateVm model)
        {
            try
            {
                model.ListOfTechnologies = new SelectList(await technologyManager.GetTechnologies(), "Id", "Name");
                if (ModelState.IsValid)
                {
                    await compileManager.Update(model);
                    return RedirectToActionPermanent(nameof(CompilerController.Index));
                }
                return View(model);
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("Task", exception.Message);
                return View(model);
            }
        }

        [Authorize(Roles = "superuser")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var model = await compileManager.GetCodeBlockById<CodeBlockVm>(id);
                return View(model);
            }
            catch
            {
                return NotFound();
            }
        }

        [Authorize(Roles = "superuser")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await compileManager.DeleteById(id);
                return RedirectToActionPermanent(nameof(CompilerController.Index));
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Observe()
        {
            try
            {
                var model = new List<CodeBlockObserveVm>();
                foreach (var item in usersWebSockets)
                {
                    var user = await userManager.FindByIdAsync(item.Key);
                    var observer = (item.Value.ConnectedUserId != null) ? await userManager.FindByIdAsync(item.Value.ConnectedUserId) : null;
                    var codeBlock = await compileManager.GetCodeBlockById<CodeBlockVm>(item.Value.TaskId);
                    model.Add(new CodeBlockObserveVm
                    {
                        User = user,
                        Observer = observer,
                        CodeBlock = codeBlock,
                    });
                }
                return View(model);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Observe(string userId, int taskId)
        {
            try
            {
                var model = await compileManager.GetCodeBlockById<CodeBlockSolveVm>(taskId);
                model.isMonitoringMode = true;
                ViewData["observedUserId"] = userId;
                return View("Solve", model);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Attempts()
        {
            string userId = userManager.GetUserId(this.HttpContext.User);
            var attempts = await compileManager.GetAttempts<AttemptCodeBlock>(userId);
            return View(attempts);
        }

        #region Web Socket connection

        /// <summary>
        /// Allows to connect through web-socket as observer
        /// </summary>
        public async Task ConnectAsObserver()
        {
            try
            {
                if (HttpContext.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                    string userId = this.userManager.GetUserId(this.HttpContext.User);
                    observersWebSockets.Add(userId, new WebSocketCompilerWrapper(webSocket));
                    await EchoForObservers(HttpContext, webSocket);
                }
                else
                {
                    HttpContext.Response.StatusCode = 400;
                }
            }
            catch (Exception exception)
            {
                HttpContext.Response.StatusCode = 500;
                // Implement handling of exception
                throw exception;
            }
        }

        private async Task EchoForObservers(HttpContext context, WebSocket webSocket)
        {
            string currentUserId = userManager.GetUserId(HttpContext.User);
            var buffer = new byte[1024 * 32];
            CompilerMessage message;
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            string data = String.Empty;
            while (!result.CloseStatus.HasValue)
            {
                if (result.MessageType != WebSocketMessageType.Text)
                {
                    throw new Exception("Message type of web-socket isn't text");
                }
                data += Encoding.UTF8.GetString(buffer, 0, result.Count);
                if (result.EndOfMessage)
                {
                    message = JsonConvert.DeserializeObject<CompilerMessage>(data);
                    message.SenderUserId = currentUserId;
                    buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                    switch (message.Type)
                    {
                        case CompilerMessageType.StartedObserve:
                            await BroadcastingFor(observersWebSockets, currentUserId, buffer, result);
                            await SendFor(usersWebSockets, message.ObservedUserId, currentUserId, buffer, result);
                            SetupObserverFor(message.ObservedUserId, currentUserId);
                            break;
                        case CompilerMessageType.FinishedObserve:
                            string observedUserId = GetConnectedUserIdFor(observersWebSockets, currentUserId);
                            if (observedUserId != null)
                            {
                                message = new CompilerMessage();
                                message.SenderUserId = currentUserId;
                                message.Type = CompilerMessageType.FinishedObserve;
                                message.ObservedUserId = observedUserId;
                                buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                                WebSocketReceiveResult receiveResult = new WebSocketReceiveResult(buffer.Length, WebSocketMessageType.Text, true);
                                await BroadcastingFor(observersWebSockets, currentUserId, buffer, receiveResult);
                                await SendFor(usersWebSockets, observedUserId, currentUserId, buffer, receiveResult);
                                DeleteObserverFor(observedUserId);
                            }
                            break;
                        case CompilerMessageType.MadeChanges:
                            string recepientId = GetConnectedUserIdFor(observersWebSockets, currentUserId);
                            if (recepientId == null) break;
                            await SendFor(usersWebSockets, recepientId, currentUserId, buffer, result);
                            break;
                        case CompilerMessageType.StartedSolveTask:
                        case CompilerMessageType.FinishedSolveTask:
                            break;
                        default:
                            await BroadcastingFor(usersWebSockets, currentUserId, buffer, result);
                            await BroadcastingFor(observersWebSockets, currentUserId, buffer, result);
                            break;
                    }
                    data = String.Empty; // Clear data string buffer
                }
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            // Close the web-socket and remove observer connection
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            observersWebSockets.Remove(currentUserId);
        }

        /// <summary>
        /// Allows to connect through web-socket as user
        /// </summary>
        public async Task ConnectAsUser()
        {
            try
            {
                if (HttpContext.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                    string userId = this.userManager.GetUserId(this.HttpContext.User);
                    usersWebSockets.Add(userId, new WebSocketCompilerWrapper(webSocket));
                    await EchoForUsers(HttpContext, webSocket);
                }
                else
                {
                    HttpContext.Response.StatusCode = 400;
                }
            }
            catch (Exception exception)
            {
                HttpContext.Response.StatusCode = 500;
                // Implement handling of exception
                throw exception;
            }
        }

        private async Task EchoForUsers(HttpContext context, WebSocket webSocket)
        {
            string currentUserId = userManager.GetUserId(HttpContext.User);
            var buffer = new byte[1024 * 32];
            CompilerMessage message;
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            string data = String.Empty;
            while (!result.CloseStatus.HasValue)
            {
                if (result.MessageType != WebSocketMessageType.Text)
                {
                    throw new Exception("Message type of web-socket isn't text");
                }
                data += Encoding.UTF8.GetString(buffer, 0, result.Count);
                if (result.EndOfMessage)
                {
                    message = JsonConvert.DeserializeObject<CompilerMessage>(data);
                    message.SenderUserId = currentUserId;
                    buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                    switch (message.Type)
                    {
                        case CompilerMessageType.StartedSolveTask:
                            var user = usersWebSockets.FirstOrDefault(u => u.Key == currentUserId);
                            if (user.Key == null) break;
                            user.Value.TaskId = message.TaskId;
                            await BroadcastingFor(observersWebSockets, currentUserId, buffer, result);
                            break;
                        case CompilerMessageType.FinishedSolveTask:
                            message = new CompilerMessage();
                            message.SenderUserId = currentUserId;
                            message.Type = CompilerMessageType.FinishedSolveTask;
                            buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                            WebSocketReceiveResult receiveResult = new WebSocketReceiveResult(buffer.Length, WebSocketMessageType.Text, true);
                            string observerUserId = GetConnectedUserIdFor(usersWebSockets, currentUserId);
                            if (observerUserId != null)
                            {
                                DeleteObserverFor(currentUserId);
                            }
                            await BroadcastingFor(observersWebSockets, currentUserId, buffer, receiveResult);
                            break;
                        case CompilerMessageType.MadeChanges:
                            string observerId = GetConnectedUserIdFor(usersWebSockets, currentUserId);
                            if (observerId == null) break;
                            await SendFor(observersWebSockets, observerId, currentUserId, buffer, result);
                            break;
                        case CompilerMessageType.StartedObserve:
                        case CompilerMessageType.FinishedObserve:
                            break;
                        case CompilerMessageType.Sync:
                            observerId = GetConnectedUserIdFor(usersWebSockets, currentUserId);
                            if (observerId == null) break;
                            await SendFor(observersWebSockets, observerId, currentUserId, buffer, result);
                            break;
                        default:
                            await BroadcastingFor(usersWebSockets, currentUserId, buffer, result);
                            await BroadcastingFor(observersWebSockets, currentUserId, buffer, result);
                            break;
                    }
                    data = String.Empty; // Clear data string buffer
                }
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            // Close the web-socket and remove user connection
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            usersWebSockets.Remove(currentUserId);
        }

        /// <summary>
        /// Setup observer for user
        /// </summary>
        /// <param name="userId">Id of observed user</param>
        /// <param name="observerId">Observer id</param>
        private void SetupObserverFor(string userId, string observerId)
        {
            var observer = observersWebSockets.FirstOrDefault(item => item.Key == observerId);
            if (observer.Key != null)
            {
                observer.Value.ConnectedUserId = userId;
            }
            var user = usersWebSockets.FirstOrDefault(item => item.Key == userId);
            if (user.Key != null)
            {
                user.Value.ConnectedUserId = observerId;
            }
        }

        /// <summary>
        /// Delete observer for user
        /// </summary>
        /// <param name="userId">Id of observed user</param>
        private void DeleteObserverFor(string userId)
        {
            var observer = observersWebSockets.FirstOrDefault(item => item.Value.ConnectedUserId == userId);
            if (observer.Key != null)
            {
                observer.Value.ConnectedUserId = null;
            }
            var user = usersWebSockets.FirstOrDefault(item => item.Key == userId);
            if (user.Key != null)
            {
                user.Value.ConnectedUserId = null;
            }
        }

        /// <summary>
        /// Send message to the whole group of recepients
        /// </summary>
        /// <param name="recepients"></param>
        /// <param name="fromUserId"></param>
        /// <param name="data"></param>
        /// <param name="receiveResult"></param>
        /// <returns></returns>
        private async Task BroadcastingFor(Dictionary<string, WebSocketCompilerWrapper> recepients, string fromUserId, byte[] data, WebSocketReceiveResult receiveResult)
        {
            foreach (var item in recepients)
            {
                //if (item.Key != fromUserId)
                //{
                    await item.Value.Socket.SendAsync(new ArraySegment<byte>(data), receiveResult.MessageType, receiveResult.EndOfMessage, CancellationToken.None);
                //}
            }
        }

        /// <summary>
        /// Send message to the specific user
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="forUserId"></param>
        /// <param name="fromUserId"></param>
        /// <param name="data"></param>
        /// <param name="receiveResult"></param>
        /// <returns></returns>
        private async Task SendFor(Dictionary<string, WebSocketCompilerWrapper> dictionary, string forUserId, string fromUserId, byte[] data, WebSocketReceiveResult receiveResult)
        {
            var recepient = dictionary.FirstOrDefault(item => item.Key == forUserId);
            if (recepient.Key != null)
            {
                await recepient.Value.Socket.SendAsync(new ArraySegment<byte>(data), receiveResult.MessageType, receiveResult.EndOfMessage, CancellationToken.None);
            }
        }

        /// <summary>
        /// Allows to get connected user's identifier
        /// </summary>
        /// <param name="dictionary">Dictionary of websockets</param>
        /// <param name="userId">Current identifier of user</param>
        /// <returns>Identifier of connected user</returns>
        private string GetConnectedUserIdFor(Dictionary<string, WebSocketCompilerWrapper> dictionary, string userId)
        {
            var member = dictionary.FirstOrDefault(item => item.Key == userId);
            if (member.Key == null)
            {
                return null;
            }
            return member.Value.ConnectedUserId;
        }

        #endregion
    }
}