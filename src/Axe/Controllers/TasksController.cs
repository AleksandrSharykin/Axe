using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Axe.Models;

namespace Axe.Controllers
{
    public class TasksController : Controller
    {
        private readonly AxeDbContext _context;

        public TasksController(AxeDbContext context)
        {
            _context = context;    
        }

        // GET: ExamTasks
        public async Task<IActionResult> Index()
        {
            var axeDbContext = _context.ExamTask.Include(e => e.Technology);
            return View(await axeDbContext.ToListAsync());
        }

        // GET: ExamTasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var examTask = await _context.ExamTask
                .Include(e => e.Technology)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (examTask == null)
            {
                return NotFound();
            }

            return View(examTask);
        }

        // GET: ExamTasks/Edit/5
        public async Task<IActionResult> Input(int? id, int? technologyId = null)
        {
            var examTask = await _context.ExamTask.SingleOrDefaultAsync(m => m.Id == id)
                           ?? new ExamTask();

            if (technologyId.HasValue)
            {
                examTask.TechnologyId = technologyId.Value;
            }

            ViewData["TechnologyId"] = new SelectList(_context.Technology, "Id", "Name", technologyId);
            return View(examTask);
        }

        // POST: ExamTasks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Input(int id, ExamTask examTask)
        {
            if (id != examTask.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (examTask.Id > 0)
                    {
                        _context.Update(examTask);
                    }
                    else
                    {
                        _context.Add(examTask);
                    }
                    
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExamTaskExists(examTask.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Technologies", new { technologyId = examTask.TechnologyId });
            }

            ViewData["TechnologyId"] = new SelectList(_context.Technology, "Id", "Name", examTask.TechnologyId);
            return View(examTask);
        }

        // GET: ExamTasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var examTask = await _context.ExamTask
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
            var examTask = await _context.ExamTask.SingleOrDefaultAsync(m => m.Id == id);
            _context.ExamTask.Remove(examTask);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ExamTaskExists(int id)
        {
            return _context.ExamTask.Any(e => e.Id == id);
        }
    }
}
