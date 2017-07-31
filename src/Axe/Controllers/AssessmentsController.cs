using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Axe.Models;
using Axe.Models.AssessmentsVm;
using Microsoft.AspNetCore.Authorization;

namespace Axe.Controllers
{
    [Authorize]
    public class AssessmentsController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AxeDbContext context;

        public AssessmentsController(UserManager<ApplicationUser> userManager, AxeDbContext context)
        {
            this.userManager = userManager;
            this.context = context;    
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return userManager.GetUserAsync(HttpContext.User);
        }

        // GET: Assessments
        public async Task<IActionResult> Index()
        {
            var axeDbContext = context.SkillAssessment.Include(s => s.Examiner).Include(s => s.Student).Include(s => s.Technology);
            return View(await axeDbContext.ToListAsync());
        }

        // GET: Assessments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var a = await context.SkillAssessment
                .Include(s => s.Examiner)
                .Include(s => s.Student)
                .Include(s => s.Technology)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (a == null)
            {
                return NotFound();
            }

            return View(new AssessmentDetailsVm
            {
                Id = a.Id,
                Student = a.Student,
                Examiner = a.Examiner,
                Technology = a.Technology,
                ExamScore = a.ExamScore,
                ExamDate = a.ExamDate,
                IsPassed = a.IsPassed,
                CanMark = a.ExamScore == null,
                CanDelete = a.ExamScore == null
            });
        }

        [HttpPost]
        public async Task<IActionResult> Details(int id, AssessmentDetailsVm vm, string cmd = null)
        {
            if (ModelState.IsValid)
            {
                var assessment = this.context.SkillAssessment.SingleOrDefault(a => a.Id == id);
                if (assessment != null)
                {
                    assessment.ExamScore = vm.ExamScore;
                    switch(cmd)
                    {
                        case "success": assessment.IsPassed = true; break;
                        case "failure": assessment.IsPassed = false; break;
                    }
                    this.context.Update(assessment);
                    await this.context.SaveChangesAsync();
                }
            }
            return await Details(vm?.Id);
        }

        private async Task<AssessmentCreateVm> GetAssessmentCreateModel(int? technologyId = null, string studentId = null, string examinerId = null)
        {
            var profile = await GetCurrentUserAsync();
            var users = await context.Users.ToListAsync();
            var data = new AssessmentCreateVm
            {
                Technologies = new SelectList(context.Technology, "Id", "Name", technologyId),
                Students = new SelectList(users, "Id", "UserName", studentId),
                Examiners = new SelectList(users, "Id", "UserName", examinerId ?? profile.Id),
            };
            return data;
        }

        // GET: Assessments/Create
        public async Task<IActionResult> Create(int? technologyId = null, string studentId = null)
        {
            var data = await GetAssessmentCreateModel(technologyId, studentId);
            return View(data);
        }

        // POST: Assessments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StudentId,ExaminerId,TechnologyId,ExamDate,ExamTime")] AssessmentCreateVm a)
        {
            if (ModelState.IsValid)
            {
                var assessment = new SkillAssessment()
                {
                    StudentId = a.StudentId,
                    ExaminerId = a.ExaminerId,                    
                    TechnologyId = a.TechnologyId.Value,
                    ExamDate = a.ExamDate.Value.Date.Add(a.ExamTime.Value.TimeOfDay),
                };
                context.Add(assessment);
                await context.SaveChangesAsync();
                return RedirectToAction("Index");
            }            

            var data = await GetAssessmentCreateModel(a.TechnologyId, a.StudentId, a.ExaminerId);
            return View(data);
        }

        // GET: Assessments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skillAssessment = await context.SkillAssessment.SingleOrDefaultAsync(m => m.Id == id);
            if (skillAssessment == null)
            {
                return NotFound();
            }
            ViewData["ExaminerId"] = new SelectList(context.Users, "Id", "Id", skillAssessment.ExaminerId);
            ViewData["StudentId"] = new SelectList(context.Users, "Id", "Id", skillAssessment.StudentId);
            ViewData["TechnologyId"] = new SelectList(context.Technology, "Id", "InformationText", skillAssessment.TechnologyId);
            return View(skillAssessment);
        }

        // POST: Assessments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StudentId,ExaminerId,TechnologyId,ExamDate,ExamScore,IsPassed")] SkillAssessment skillAssessment)
        {
            if (id != skillAssessment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    context.Update(skillAssessment);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SkillAssessmentExists(skillAssessment.Id))
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
            ViewData["ExaminerId"] = new SelectList(context.Users, "Id", "Id", skillAssessment.ExaminerId);
            ViewData["StudentId"] = new SelectList(context.Users, "Id", "Id", skillAssessment.StudentId);
            ViewData["TechnologyId"] = new SelectList(context.Technology, "Id", "InformationText", skillAssessment.TechnologyId);
            return View(skillAssessment);
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

        private bool SkillAssessmentExists(int id)
        {
            return context.SkillAssessment.Any(e => e.Id == id);
        }
    }
}
