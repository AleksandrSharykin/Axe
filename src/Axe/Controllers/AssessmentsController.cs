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
    public class AssessmentsController : ControllerExt
    {
        public AssessmentsController(UserManager<ApplicationUser> userManager, AxeDbContext context) : base(userManager, context) { }

        // GET: Assessments
        public async Task<IActionResult> Index()
        {
            var axeDbContext = context.SkillAssessment.Include(s => s.Examiner).Include(s => s.Student).Include(s => s.Technology);
            return View(await axeDbContext.ToListAsync());
        }

        /// <summary>
        /// Loads <see cref="SkillAssessment"/> item with all properties
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<SkillAssessment> GetAssessment(int? id)
        {
            return await context.SkillAssessment
                                .Include(s => s.Examiner)
                                .Include(s => s.Student)
                                .Include(s => s.Technology)
                                .SingleOrDefaultAsync(m => m.Id == id);
        }

        /// <summary>
        /// Creates view model for <see cref="SkillAssessment"/> item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<AssessmentDetailsVm> GetAssessmentDetails(int? id)
        {
            if (id == null)
            {
                return null;
            }

            var a = await this.GetAssessment(id);
            if (a == null)
            {
                return null;
            }

            var user = await this.GetCurrentUserAsync();

            return new AssessmentDetailsVm
            {
                Id = a.Id,
                Student = a.Student,
                Examiner = a.Examiner,
                Technology = a.Technology,
                ExamScore = a.ExamScore,
                ExamComment = a.ExamComment,
                ExamDate = a.ExamDate,
                IsPassed = a.IsPassed,
                CanMark = a.IsPassed == null && user.Id == a.ExaminerId,
                CanEdit = a.IsPassed == null,
                CanDelete = a.IsPassed == null,
            };
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
            SkillAssessment data = null;
            if (id.HasValue)
            {
                data = await context.SkillAssessment.SingleOrDefaultAsync(m => m.Id == id);
            }

            ApplicationUser currentUser = await GetCurrentUserAsync();

            Technology technology = null;
            ApplicationUser student = null;
            ApplicationUser examiner = null;

            if (data != null)
            {
                // loading properties for an existing assessment
                examiner = await this.context.Users.SingleAsync(u => u.Id == data.ExaminerId);
                student = await this.context.Users.SingleAsync(u => u.Id == data.StudentId);
                technology = await this.context.Technology.Include(t => t.Experts).SingleAsync(t => t.Id == data.TechnologyId);
                if (examiner.Id != currentUser.Id)
                {
                    examiner = null;
                }
            }
            else
            {
                // creating properties for a new assessment
                examiner = currentUser;
                if (studentId != null)
                {
                    student = await this.context.Users.SingleAsync(u => u.Id == studentId);
                }
                if (technologyId.HasValue)
                {
                    technology = await this.context.Technology.Include(t => t.Experts).SingleAsync(t => t.Id == technologyId);
                }
            }

            if (examiner == null || student == null || technology == null ||
                false == technology.Experts.Any(u => u.UserId == examiner.Id))
            {
                return RedirectToAction("Visit", "Profiles", new { id = student?.Id, technologyId = technology?.Id });
            }

            // creating view model to display
            var vm = new AssessmentInputVm
            {
                TechnologyId = technology.Id,
                TechnologyName = technology.Name,

                ExaminerId = examiner.Id,
                ExaminerName = examiner.UserName,

                StudentId = student.Id,
                StudentName = student.UserName,
            };

            if (data != null)
            {
                vm.Id = data.Id;
                vm.ExamDate = data.ExamDate;
                vm.ExamTime = data.ExamDate;
            }

            return View(vm);
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
            var technology = await this.context.Technology
                                       .Include(t => t.Experts)
                                       .SingleAsync(t => t.Id == assessmentInput.TechnologyId);

            if (technology == null)
            {
                ModelState.AddModelError(String.Empty, "Unknown technology");
            }

            if (ModelState.IsValid)
            {
                var currentUser = await GetCurrentUserAsync();                

                var a = await context.SkillAssessment.SingleOrDefaultAsync(e => e.Id == assessmentInput.Id);

                if (a == null)
                {
                    // validation for create
                    if (false == technology.Experts.Any(u => u.UserId == currentUser.Id))
                    {
                        ModelState.AddModelError(String.Empty, "Only " + technology.Name + "expert can assign skill assessment");
                    }
                    else if (assessmentInput.StudentId == currentUser.Id)
                    {
                        ModelState.AddModelError(String.Empty, "Cannot assign skill assessment to self");
                    }
                    else
                    {
                        a = new SkillAssessment
                        {
                            ExaminerId = currentUser.Id,
                            StudentId = assessmentInput.StudentId,
                            TechnologyId = assessmentInput.TechnologyId,
                        };
                    }
                }
                else
                {
                    // validation for update
                    if (a.IsPassed.HasValue)
                    {
                        ModelState.AddModelError(String.Empty, "Event has already happened");
                    }

                    if (a.ExaminerId   != assessmentInput.ExaminerId ||
                        a.StudentId    != assessmentInput.StudentId  ||
                        a.TechnologyId != assessmentInput.TechnologyId)
                    {
                        ModelState.AddModelError(String.Empty, "Invalid assessment details");
                    }
                }

                // commit changes
                if (ModelState.IsValid)
                {
                    a.ExamDate = assessmentInput.ExamDate.Value.Date.Add(assessmentInput.ExamTime.Value.TimeOfDay);

                    if (a.Id > 0)
                    {
                        this.context.Update(a);
                    }
                    else
                    {
                        this.context.Add(a);
                    }
                    await context.SaveChangesAsync();

                    return RedirectToAction("Details", new { id = a.Id });
                }
            }

            // restoring view model displayed properties
            
            assessmentInput.TechnologyName = technology?.Name;

            var student = await this.context.Users.SingleOrDefaultAsync(u => u.Id == assessmentInput.StudentId);
            assessmentInput.StudentName = student?.UserName;

            var examiner = await this.context.Users.SingleOrDefaultAsync(u => u.Id == assessmentInput.ExaminerId);
            assessmentInput.ExaminerName = examiner?.UserName;

            return View(assessmentInput);
        }

        /// <summary>
        /// Displays information about <see cref="SkillAssessment"/> item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            var details = await GetAssessmentDetails(id);
            if (details == null)
            {
                return NotFound();
            }

            return View(details);
        }

        /// <summary>
        /// Displays information for Exminer to mark assessment
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Mark(int? id)
        {
            var details = await GetAssessmentDetails(id);

            if (details == null || false == details.CanMark)
            {
                return NotFound();
            }

            return View(details);
        }

        /// <summary>
        /// Applies exminer mark to asessment
        /// </summary>
        /// <param name="id"></param>
        /// <param name="vm"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Mark(int id, AssessmentDetailsVm vm, string cmd = null)
        {
            var assessment = await this.GetAssessment(id);
            if (assessment == null)
            {
                return NotFound();
            }

            var user = await this.GetCurrentUserAsync();
            if (assessment.ExaminerId != user.Id)
            {
                ModelState.AddModelError(String.Empty, "Only examiner can mark assessment");
            }

            if (ModelState.IsValid)
            {
                assessment.ExamScore = vm.ExamScore;
                assessment.ExamComment = vm.ExamComment;
                switch (cmd)
                {
                    case "success": assessment.IsPassed = true; break;
                    case "failure": assessment.IsPassed = false; break;                    
                }
                if (assessment.IsPassed.HasValue)
                {
                    this.context.Update(assessment);
                    await this.context.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = assessment.Id });
                }
            }
            
            var a = await GetAssessment(assessment.Id);
            vm.Student = a.Student;
            vm.Examiner = a.Examiner;
            vm.Technology = a.Technology;                        
            vm.ExamDate = a.ExamDate;
            return View(vm);
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
