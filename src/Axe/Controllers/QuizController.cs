using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.WebSockets;
using Axe.Dto;
using Axe.Models;
using Axe.Managers;

namespace Axe.Controllers
{
    [Authorize]
    public class QuizController : ControllerExt
    {
        private IQuizManager manager;

        public QuizController(UserManager<ApplicationUser> userManager, IQuizManager manager) : base(userManager, null)
        {
            this.manager = manager;
        }

        /// <summary>
        /// Returns a list of available quizes 
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var request = await this.CreateRequest(0);

            var response = await this.manager.Index();

            ViewData["UserId"] = request.CurrentUser.Id;
            return View(response.Item);
        }

        /// <summary>
        /// Returns information about quiz participants
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int id)
        {
            var request = await this.CreateRequest(id);

            var response = await this.manager.Details(request);

            return Json(response.Item);
        }

        /// <summary>
        /// Returns quiz instance for edit
        /// </summary>
        /// <param name="id"></param>        
        [HttpGet]
        public async Task<IActionResult> Input(int? id = null)
        {
            var request = await this.CreateRequest(id);

            var response = await this.manager.InputGet(request);

            return View(response.Item);
        }

        /// <summary>
        /// Validates and saves quiz edits
        /// </summary>
        /// <param name="id"></param>        
        [HttpPost]
        public async Task<IActionResult> Input(int? id, RealtimeQuiz quizInput)
        {
            var request = await this.CreateRequest(quizInput);

            var response = await this.manager.InputPost(request);

            if (response.Code == ResponseCode.NotFound)
            {
                return NotFound();
            }

            if (response.Code == ResponseCode.Success)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(quizInput);
        }

        /// <summary>
        /// Open a page for a judge to ask questions
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Ask(int id)
        {
            var request = await this.CreateRequest(id);

            var response = await this.manager.Ask(request);

            if (response.Code == ResponseCode.ValidationError)
            {
                return RedirectToAction(nameof(Answer), new { id });
            }

            ViewData["UserId"] = request.CurrentUser.Id;
            return View(response.Item);
        }

        /// <summary>
        /// Open page for participant to answer questions
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Answer(int id)
        {
            var request = await this.CreateRequest(id);

            var response = await this.manager.Answer(request);

            ViewData["UserId"] = request.CurrentUser.Id;
            return View(response.Item);
        }

        /// <summary>
        /// Accepts WebSocket communication requests
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> Participate()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                var request = await this.CreateRequest(webSocket);

                await this.manager.Connect(request);
            }

            return Json(true);
        }

        /// <summary>
        /// Marks an answer from participant
        /// </summary>
        /// <param name="message">Message from participant with an answer</param>
        /// <returns></returns>
        public async Task<IActionResult> Mark(QuizMessage message)
        {
            var request = await this.CreateRequest(message);

            var response = await this.manager.Mark(request);

            return Json(response.Item);
        }
    }
}