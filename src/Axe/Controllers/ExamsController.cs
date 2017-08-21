using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Axe.Models;
using Axe.Managers;

namespace Axe.Controllers
{
    public class ExamsController : ControllerExt
    {
        private IExamManager manager;

        public ExamsController(UserManager<ApplicationUser> userManager, IExamManager manager)
             : base(userManager, null)
        {
            this.manager = manager;
        }

        // GET: Exams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await this.GetCurrentUserAsync();

            // redirect registered users to their profile page where they can select exam
            if (user != null)
            {
                return RedirectToAction("Visit", "Profiles", new { id = user.Id, technologyId = id });
            }

            return RedirectToAction("Take", "Exams", new { technologyId = id });
        }

        [HttpGet]
        public async Task<IActionResult> Take(int? id = null, int? taskId = null, int? technologyId = null)
        {
            var request = await this.CreateRequest(new ExamAttempt { Id = id ?? 0, TaskId = taskId ?? 0, TechnologyId = technologyId });

            var response = await this.manager.AttemptGet(request);

            if (response.Code == ResponseCode.NotFound)
            {
                return NotFound();
            }

            if (response.Item.IsFinished)
            {
                return RedirectToAction("Result", new { id = response.Item.Id });
            }

            return View(response.Item);
        }

        [HttpPost]
        public async Task<IActionResult> Take(ExamAttempt attempt, string cmd = null)
        {
            if (ModelState.IsValid)
            {
                var request = await CreateRequest(attempt);

                attempt.IsFinished = cmd != null;

                var response = await this.manager.AttemptPost(request);

                if (response.Code == ResponseCode.NotFound)
                {
                    return this.NotFound();
                }

                if (response.Item.IsFinished)
                {
                    return View("Result", response.Item);
                }

                return View(response.Item);
            }

            return View(attempt);
        }

        [HttpPost]
        public async Task<ExamAttempt> Monitor(ExamAttempt attempt)
        {
            if (ModelState.IsValid)
            {
                var request = await CreateRequest(attempt);

                attempt.IsFinished = false;

                var response = await this.manager.AttemptPost(request);

            }
            return attempt;
        }

        public async Task<IActionResult> Result(int id)
        {
            var request = await this.CreateRequest(id);

            var response = await this.manager.Results(request);

            if (response.Code == ResponseCode.NotFound)
            {
                return this.NotFound();
            }

            return this.View(response.Item);
        }

        /// <summary>
        /// Loads exam attemps details for preview before deletion
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await CreateRequest(id.Value);

            var details = await this.manager.DeletePreview(request);

            if (details.Code == ResponseCode.NotFound)
            {
                return NotFound();
            }

            return View(details.Item);
        }

        /// <summary>
        /// Deletes exam attempt
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var request = await CreateRequest(id);

            var response = await this.manager.Delete(request);

            return RedirectToAction("Visit", "Profiles");
        }
    }
}
