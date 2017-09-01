using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Axe.Models;
using Axe.Models.AssessmentsVm;
using Axe.Managers;
using Axe.Dto;

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
        /// Return information about skill assessment for ajax details request
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> Item(int? id)
        {
            var request = await this.CreateRequest(id ?? 0);

            var response = await this.manager.DetailsGet(request);

            var details = response.Item;
            if (details == null)
            {
                return Json(null);
            }

            var result = new AssessmentDetails
            {
                ExaminerName = details.Examiner?.UserName,
                TechnologyName = details.Technology?.Name,
                ExamDate = details.ExamDate?.ToString(),
                ExamScore = details.ExamScore,
                ExamComment = details.ExamComment,
                IsPassed = details.IsPassed,
                CanEdit = details.CanEdit,
                CanMark = details.CanMark,
                CanDelete = details.CanDelete,
            };
            return Json(result);
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
                case "success":
                vm.IsPassed = true;
                break;
                case "failure":
                vm.IsPassed = false;
                break;
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

        /// <summary>
        /// Gets <see cref="SkillAssessment"/> for preview before deletion
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await this.CreateRequest(id.Value);

            var response = await this.manager.DeleteGet(request);

            if (response.Code == ResponseCode.Success)
            {
                return View(response.Item);
            }

            return this.NotFound();
        }

        /// <summary>
        /// Deletes <see cref="SkillAssessment"/>
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var request = await this.CreateRequest(id);

            var response = await this.manager.DeletePost(request);

            if (response.Code == ResponseCode.Success)
            {
                return RedirectToAction(nameof(ProfilesController.Visit), "Profiles", new { id = response.Item.StudentId ?? response.Item.Student.Id });
            }

            if (response.Code == ResponseCode.ValidationError)
            {
                return View(nameof(Delete), response.Item);
            }

            return this.NotFound();
        }
    }
}
