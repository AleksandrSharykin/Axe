using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Axe.Models;
using Axe.Models.QuestionsVm;
using Axe.Managers;

namespace Axe.Controllers
{
    [Authorize]
    public class QuestionsController : ControllerExt
    {
        private IQuestionManager manager;

        public QuestionsController(UserManager<ApplicationUser> userManager, IQuestionManager manager) : base(userManager, null)
        {
            this.manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> Input(int? id = null, int? technologyId = null, int? questionType = null)
        {
            var q = new TaskQuestion
            {
                Id = id ?? 0,
                TechnologyId = technologyId,
                Type = (TaskQuestionType)(questionType ?? -1)
            };

            var request = await this.CreateRequest(q);

            var response = await this.manager.InputGet(request);

            if (response.Code == ResponseCode.NotFound)
            {
                return RedirectToAction("Index", "Technologies");
            }

            return View(response.Item);
        }

        [HttpPost]
        public async Task<IActionResult> Input(int id, QuestionInputVm questionVm, int? etp = null, string cmd = null)
        {
            cmd = cmd?.Trim()?.ToLower();

            var request = await this.CreateRequest(questionVm);

            Response<QuestionInputVm> response;

            if (cmd == "add")
            {
                response = this.manager.ChangeAnswers(request, true);
            }
            else if (cmd == "remove")
            {
                response = this.manager.ChangeAnswers(request, false);
            }
            else if (etp.HasValue)
            {
                response = this.manager.ChangeQuestionType(request, (TaskQuestionType)etp.Value);
            }
            else
            {
                response = await this.manager.InputPost(request);

                if (response.Code == ResponseCode.Success)
                {
                    return RedirectToAction("Index", "Technologies", new { technologyId = response.Item.TechnologyId });
                }
            }

            if (response.Code == ResponseCode.NotFound)
            {
                return NotFound();
            }

            return View(response.Item);
        }

        /// <summary>
        /// Displays details about <see cref="TaskQuestion"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int id)
        {
            var request = await this.CreateRequest(id);

            var response = await this.manager.DetailsGet(request);

            if (response.Code == ResponseCode.NotFound)
            {
                return this.NotFound();
            }

            return View(response.Item);
        }
    }
}
