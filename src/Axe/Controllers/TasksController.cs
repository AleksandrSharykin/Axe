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
using Axe.Models.ExamTasksVm;
using Axe.Managers;

namespace Axe.Controllers
{
    [Authorize]
    public class TasksController : ControllerExt
    {
        private IExamTaskManager manager;

        public TasksController(UserManager<ApplicationUser> userManager, IExamTaskManager manager) : base(userManager, null)
        {
            this.manager = manager;
        }

        public async Task<IActionResult> Details(int? id)
        {
            var request = await this.CreateRequest(id);
            var response = await this.manager.DetailsGet(request);

            if (response.Code == ResponseCode.NotFound)
            {
                return NotFound();
            }

            return View(response.Item);
        }

        public async Task<IActionResult> Input(int? id, int? technologyId = null)
        {
            var request = await this.CreateRequest(new ExamTask { Id = id ?? 0, TechnologyId = technologyId });

            var response = await this.manager.InputGet(request);

            if (response.Code == ResponseCode.Success)
            {
                return View(response.Item);
            }

            return this.NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Input(int id, TaskInputVm taskInput)
        {
            if (id != taskInput.Id)
            {
                return NotFound();
            }

            var request = await this.CreateRequest(taskInput);

            var response = await this.manager.InputPost(request);

            if (response.Code == ResponseCode.Success)
            {
                return RedirectToAction("Index", "Technologies", new { technologyId = response.Item.TechnologyId });
            }

            return View(response.Item);
        }

        // GET: ExamTasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var examTask = await context.ExamTask
                .Include(e => e.Technology)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (examTask == null)
            {
                return NotFound();
            }

            return View(examTask);
        }

        // POST: ExamTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var examTask = await context.ExamTask.SingleOrDefaultAsync(m => m.Id == id);
            context.ExamTask.Remove(examTask);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ExamTaskExists(int id)
        {
            return context.ExamTask.Any(e => e.Id == id);
        }
    }
}
