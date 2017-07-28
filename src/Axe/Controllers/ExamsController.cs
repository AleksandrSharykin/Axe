using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Axe.Models;

namespace Axe.Controllers
{
    public class ExamsController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AxeDbContext _context;

        public ExamsController(UserManager<ApplicationUser> userManager, AxeDbContext context)
        {
            _context = context;
            this.userManager = userManager;
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return userManager.GetUserAsync(HttpContext.User);
        }

        // GET: Exams
        public async Task<IActionResult> Index()
        {
            var axeDbContext = _context.ExamAttempt.Include(e => e.Student).Include(e => e.Task).Include(e => e.Technology);
            return View(await axeDbContext.ToListAsync());
        }

        // GET: Exams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await GetCurrentUserAsync();
            // redirect registered users to their profile page where they can select exam
            if (user != null)
                return RedirectToAction("Visit", "Profiles", new { id = user.Id, technologyId = id });

            // redirect anonymous users to sample exam
            var examAttempt = await _context.ExamAttempt            
                .FirstOrDefaultAsync(m => m.TechnologyId == id);

            return RedirectToAction("Take", "Exams", new { id = examAttempt?.Id });
        }

        public async Task<IActionResult> Take(int? id)
        {
            // redirect anonymous users to sample exam
            var examAttempt = await _context.ExamAttempt
                .Include(e => e.Student)
                .Include(e => e.Task)
                .Include(e => e.Technology)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (examAttempt == null)
            {                
                examAttempt = new ExamAttempt()
                {
                    Task = new ExamTask { Technology = new Technology { Name = "Demo" }, Title = "Exam demonstration", Objective = "Introduction to the Axe skill assessment system" },                    
                    Questions = new List<AttemptQuestion>
                    {
                        new AttemptQuestion
                        {
                            TaskQuestion = new TaskQuestion
                            {
                                Text = "Do you like Axe application?",
                                Answers = new List<TaskAnswer>
                                {
                                    new TaskAnswer { Text = "YES", Value = "true"},
                                    new TaskAnswer { Text = "no", Value = "false"},
                                }
                            }
                        },
                        new AttemptQuestion
                        {
                            TaskQuestion = new TaskQuestion
                            {
                                Text = "Have you seen any bugs?",
                                Answers = new List<TaskAnswer>
                                {
                                    new TaskAnswer { Text = "yes", Value = "false"},
                                    new TaskAnswer { Text = "NO", Value = "true"},
                                }
                            }
                        }
                    }
                };
            }

            return View(examAttempt);
        }

        // GET: Exams/Create
        public IActionResult Create()
        {
            ViewData["StudentId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["TaskId"] = new SelectList(_context.ExamTask, "Id", "Id");
            ViewData["TechnologyId"] = new SelectList(_context.Technology, "Id", "InformationText");
            return View();
        }

        // POST: Exams/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TechnologyId,TaskId,StudentId,ExamDate,ExamScore,IsPassed")] ExamAttempt examAttempt)
        {
            if (ModelState.IsValid)
            {
                _context.Add(examAttempt);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["StudentId"] = new SelectList(_context.Users, "Id", "Id", examAttempt.StudentId);
            ViewData["TaskId"] = new SelectList(_context.ExamTask, "Id", "Id", examAttempt.TaskId);
            ViewData["TechnologyId"] = new SelectList(_context.Technology, "Id", "InformationText", examAttempt.TechnologyId);
            return View(examAttempt);
        }

        // GET: Exams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var examAttempt = await _context.ExamAttempt.SingleOrDefaultAsync(m => m.Id == id);
            if (examAttempt == null)
            {
                return NotFound();
            }
            ViewData["StudentId"] = new SelectList(_context.Users, "Id", "Id", examAttempt.StudentId);
            ViewData["TaskId"] = new SelectList(_context.ExamTask, "Id", "Id", examAttempt.TaskId);
            ViewData["TechnologyId"] = new SelectList(_context.Technology, "Id", "InformationText", examAttempt.TechnologyId);
            return View(examAttempt);
        }

        // POST: Exams/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TechnologyId,TaskId,StudentId,ExamDate,ExamScore,IsPassed")] ExamAttempt examAttempt)
        {
            if (id != examAttempt.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(examAttempt);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExamAttemptExists(examAttempt.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            ViewData["StudentId"] = new SelectList(_context.Users, "Id", "Id", examAttempt.StudentId);
            ViewData["TaskId"] = new SelectList(_context.ExamTask, "Id", "Id", examAttempt.TaskId);
            ViewData["TechnologyId"] = new SelectList(_context.Technology, "Id", "InformationText", examAttempt.TechnologyId);
            return View(examAttempt);
        }

        // GET: Exams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var examAttempt = await _context.ExamAttempt
                .Include(e => e.Student)
                .Include(e => e.Task)
                .Include(e => e.Technology)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (examAttempt == null)
            {
                return NotFound();
            }

            return View(examAttempt);
        }

        // POST: Exams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var examAttempt = await _context.ExamAttempt.SingleOrDefaultAsync(m => m.Id == id);
            _context.ExamAttempt.Remove(examAttempt);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ExamAttemptExists(int id)
        {
            return _context.ExamAttempt.Any(e => e.Id == id);
        }
    }
}
