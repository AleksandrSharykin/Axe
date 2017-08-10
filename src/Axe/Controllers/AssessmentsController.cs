using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Axe.Models;
using Axe.Models.AssessmentsVm;
using Axe.Managers;

namespace Axe.Controllers
{
    [Authorize]
    public class AssessmentsController : ControllerExt
    {
        private IAssessmentManager manager;

        public AssessmentsController(UserManager<ApplicationUser> userManager, IAssessmentManager manager) : base(userManager, null)
        {
            this.manager = manager;
        }

        /// <summary>
        /// Displays <see cref="SkillAssessment"/> item template to assign or change assessment time
        /// </summary>
        /// <param name="id"></param>
        /// <param name="technologyId"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Input(int? id, int? technologyId = null, string studentId = null)
        {
            var args = new SkillAssessment { Id = id ?? 0, TechnologyId = technologyId ?? 0, StudentId = studentId };

            var request = await this.CreateRequest(args);

            var response = await this.manager.InputGet(request);

            if (response.Code == ResponseCode.NotFound)
            {
                return RedirectToAction("Visit", "Profiles", new { id = studentId, technologyId = technologyId });
            }

            return View(response.Item);
        }

        /// <summary>
        /// Creates <see cref="SkillAssessment"/> or updates <see cref="SkillAssessment"/> time
        /// </summary>
        /// <param name="id"></param>
        /// <param name="assessmentInput"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Input(int id, [Bind("Id,StudentId,ExaminerId,TechnologyId,ExamDate,ExamTime")] AssessmentInputVm assessmentInput)
        {
            var request = await this.CreateRequest(assessmentInput);

            var response = await this.manager.InputPost(request);

            if (response.Code == ResponseCode.Success)
            {
                return RedirectToAction("Details", new { id = response.Item.Id });
            }

            return View(response.Item);
        }

        /// <summary>
        /// Displays information about <see cref="SkillAssessment"/> item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            var request = await this.CreateRequest(id ?? 0);

            var response = await this.manager.DetailsGet(request);

            if (response.Code == ResponseCode.NotFound)
            {
                return NotFound();
            }

            return View(response.Item);
        }

        /// <summary>
        /// Displays information for Examiner to mark assessment
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Mark(int? id)
        {
            var request = await this.CreateRequest(id ?? 0);

            var response = await this.manager.MarkGet(request);

            if (response.Code == ResponseCode.NotFound)
            {
                return NotFound();
            }

            return View(response.Item);
        }

        /// <summary>
        /// Applies examiner mark to asessment
        /// </summary>
        /// <param name="id"></param>
        /// <param name="vm"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Mark(int id, AssessmentDetailsVm vm, string cmd = null)
        {
            var request = await this.CreateRequest(vm);
            
            switch (cmd)
            {
                case "success": vm.IsPassed = true; break;
                case "failure": vm.IsPassed = false; break;
            }

            var response = await this.manager.MarkPost(request);

            if (response.Code == ResponseCode.Success)
            {
                return RedirectToAction("Details", new { id = response.Item.Id });
            }

            if (response.Code == ResponseCode.NotFound)
            {
                return NotFound();
            }

            return View(response.Item);
        }

        // GET: Assessments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skillAssessment = await context.SkillAssessment
                .Include(s => s.Examiner)
                .Include(s => s.Student)
                .Include(s => s.Technology)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (skillAssessment == null)
            {
                return NotFound();
            }

            return View(skillAssessment);
        }

        // POST: Assessments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var skillAssessment = await context.SkillAssessment.SingleOrDefaultAsync(m => m.Id == id);
            context.SkillAssessment.Remove(skillAssessment);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
