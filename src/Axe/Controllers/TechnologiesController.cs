using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Axe.Models;
using Axe.Managers;

namespace Axe.Controllers
{
    [Authorize]
    public class TechnologiesController : ControllerExt
    {
        private ITechnologyManager manager;

        public TechnologiesController(UserManager<ApplicationUser> userManager, ITechnologyManager manager) : base(userManager, null)
        {
            this.manager = manager;
        }

        /// <summary>
        /// Returns a list of technologies available for current user with details about selected technology
        /// </summary>
        /// <param name="technologyId"></param>
        /// <returns></returns>
        public async Task<IActionResult> Index(int? technologyId = null)
        {
            var request = await this.CreateRequest(technologyId);

            var response = await this.manager.Index(request);

            return View(response.Item);
        }

        /// <summary>
        /// Returns page to create or edit <see cref="Technology"/> entity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Input(int? id)
        {
            var request = await this.CreateRequest(id);

            var response = await this.manager.InputGet(request);

            if (response.Code == ResponseCode.NotFound)
            {
                return NotFound();
            }

            return View(response.Item);
        }

        /// <summary>
        /// Submits <see cref="Technology"/> entity for creation or edit
        /// </summary>
        /// <param name="id"></param>
        /// <param name="technologyInput"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Input(int? id, [Bind("Id,Name,InformationText")] Technology technologyInput)
        {
            if (id != technologyInput.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var request = await CreateRequest(technologyInput);

                var response = await this.manager.InputPost(request);

                if (response.Code == ResponseCode.NotFound)
                {
                    return NotFound();
                }

                if (response.Code == ResponseCode.Success)
                {
                    return RedirectToAction("Index", new { technologyId = technologyInput.Id });
                }
            }
            return View(technologyInput);
        }

        /// <summary>
        /// Adds a user to technology expert list
        /// </summary>
        public async Task<IActionResult> IncludeExpert(string id, int technologyId)
        {
            var request = await this.CreateRequest(new ExpertTechnologyLink { UserId = id, TechnologyId = technologyId });

            await this.manager.IncludeExpert(request);

            return RedirectToAction("Index", new { technologyId });
        }

        /// <summary>
        /// Removes a user from technology experts list
        /// </summary>
        public async Task<IActionResult> ExcludeExpert(string id, int technologyId)
        {
            var request = await this.CreateRequest(new ExpertTechnologyLink { UserId = id, TechnologyId = technologyId });

            await this.manager.ExcludeExpert(request);

            return RedirectToAction("Index", new { technologyId });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await this.CreateRequest(id.Value);

            var response = await this.manager.DeleteGet(request);

            if (response.Code == ResponseCode.NotFound)
            {
                return NotFound();
            }

            return View(response.Item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var request = await this.CreateRequest(id);

            var response = await this.manager.DeletePost(request);

            if (response.Code == ResponseCode.Success)
            {
                return RedirectToAction(nameof(Index));
            }

            if (response.Code == ResponseCode.ValidationError)
            {
                return View(nameof(Delete), response.Item);
            }

            return this.NotFound();
        }
    }
}
