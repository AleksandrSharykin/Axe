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
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AxeDbContext context;

        public ExamsController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, AxeDbContext context)
        {
            this.signInManager = signInManager;
            this.context = context;
            this.userManager = userManager;
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return userManager.GetUserAsync(HttpContext.User);
        }

        // GET: Exams
        public async Task<IActionResult> Index()
        {
            var axeDbContext = context.ExamAttempt.Include(e => e.Student).Include(e => e.Task).Include(e => e.Technology);
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

            return RedirectToAction("Take", "Exams", new { technologyId = id });
        }

        public async Task<IActionResult> Take(int? id, int? technologyId = null)
        {
            var exams = this.context.ExamTask
                .Include(t => t.Technology)
                .Include(t => t.Questions).ThenInclude(q => q.Question).ThenInclude(q => q.Answers);

            ExamTask task = null;
            
            if (id.HasValue)
                task = await exams.SingleOrDefaultAsync(t => t.Id == id);

            if (task != null && 
                false == task.IsDemonstration && 
                false == this.signInManager.IsSignedIn(HttpContext.User))
            {
                // anonymous users can only try demo exams
                return NotFound();
            }
            
            if (task == null && technologyId.HasValue)
            {
                task = exams.Where(t => t.TechnologyId == technologyId && t.IsDemonstration).FirstOrDefault();
            }

            if (task == null)
            {
                return NotFound();
            }

            var examAttempt = new ExamAttempt()
            {
                Task = task,
                TaskId = task.Id,
                Questions = task.Questions
                                .Select(q => new AttemptQuestion
                                             {
                                                 TaskQuestion = q.Question,
                                                 TaskQuestionId = q.QuestionId,
                                                 AttemptAnswers = q.Question.Answers.Select(a => new AttemptAnswer{ TaskAnswer = a, TaskAnswerId = a.Id }).ToList()
                                             })
                                .ToList()
            };
            
            return View(examAttempt);
        }

        [HttpPost]
        public async Task<IActionResult> Take(int? id, ExamAttempt attempt)
        {
            return View(attempt);
        }

        // GET: Exams/Create
        public IActionResult Create()
        {
            ViewData["StudentId"] = new SelectList(context.Users, "Id", "Id");
            ViewData["TaskId"] = new SelectList(context.ExamTask, "Id", "Id");
            ViewData["TechnologyId"] = new SelectList(context.Technology, "Id", "InformationText");
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
                context.Add(examAttempt);
                await context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["StudentId"] = new SelectList(context.Users, "Id", "Id", examAttempt.StudentId);
            ViewData["TaskId"] = new SelectList(context.ExamTask, "Id", "Id", examAttempt.TaskId);
            ViewData["TechnologyId"] = new SelectList(context.Technology, "Id", "InformationText", examAttempt.TechnologyId);
            return View(examAttempt);
        }

        // GET: Exams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var examAttempt = await context.ExamAttempt.SingleOrDefaultAsync(m => m.Id == id);
            if (examAttempt == null)
            {
                return NotFound();
            }
            ViewData["StudentId"] = new SelectList(context.Users, "Id", "Id", examAttempt.StudentId);
            ViewData["TaskId"] = new SelectList(context.ExamTask, "Id", "Id", examAttempt.TaskId);
            ViewData["TechnologyId"] = new SelectList(context.Technology, "Id", "InformationText", examAttempt.TechnologyId);
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
                    context.Update(examAttempt);
                    await context.SaveChangesAsync();
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
            ViewData["StudentId"] = new SelectList(context.Users, "Id", "Id", examAttempt.StudentId);
            ViewData["TaskId"] = new SelectList(context.ExamTask, "Id", "Id", examAttempt.TaskId);
            ViewData["TechnologyId"] = new SelectList(context.Technology, "Id", "InformationText", examAttempt.TechnologyId);
            return View(examAttempt);
        }

        // GET: Exams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var examAttempt = await context.ExamAttempt
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
            var examAttempt = await context.ExamAttempt.SingleOrDefaultAsync(m => m.Id == id);
            context.ExamAttempt.Remove(examAttempt);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ExamAttemptExists(int id)
        {
            return context.ExamAttempt.Any(e => e.Id == id);
        }
    }
}
