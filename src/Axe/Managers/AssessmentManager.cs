using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Axe.Models;
using Axe.Models.AssessmentsVm;

namespace Axe.Managers
{
    /// <summary>
    /// Class implements operations governed by AssessmentController
    /// </summary>
    public class AssessmentManager : ManagerBase, IAssessmentManager
    {
        public AssessmentManager(AxeDbContext context) : base(context)
        {
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
        private async Task<AssessmentDetailsVm> GetAssessmentDetails(string userId, int? id)
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
                CanMark = a.IsPassed == null && userId == a.ExaminerId,
                CanEdit = a.IsPassed == null && userId == a.ExaminerId,
                CanDelete = userId == a.ExaminerId,
            };
        }

        /// <summary>
        /// Gets <see cref="SkillAssessment"/> object template for creation or edit
        /// </summary>
        public async Task<Response<AssessmentInputVm>> InputGet(Request<SkillAssessment> request)
        {
            int id = request.Item.Id;
            int technologyId = request.Item.TechnologyId;
            string studentId = request.Item.StudentId;

            SkillAssessment data = null;

            if (id > 0)
            {
                data = await context.SkillAssessment.SingleOrDefaultAsync(m => m.Id == id);
            }

            ApplicationUser currentUser = request.CurrentUser;

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
                if (technologyId > 0)
                {
                    technology = await this.context.Technology.Include(t => t.Experts).SingleAsync(t => t.Id == technologyId);
                }
            }

            if (examiner == null || student == null || technology == null ||
                examiner.Id == student.Id ||
                false == technology.Experts.Any(u => u.UserId == examiner.Id))
            {
                return this.NotFound<AssessmentInputVm>();
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

            return this.Response(vm);
        }

        /// <summary>
        /// Applies <see cref="SkillAssessment"/> edit results
        /// </summary>
        public async Task<Response<AssessmentInputVm>> InputPost(Request<AssessmentInputVm> request)
        {
            var assessmentInput = request.Item;

            var technology = await this.context.Technology
                           .Include(t => t.Experts)
                           .SingleOrDefaultAsync(t => t.Id == assessmentInput.TechnologyId);

            if (technology == null)
            {
                request.ModelState.AddModelError(String.Empty, "Unknown technology");
                return this.ValidationError(assessmentInput);
            }

            if (request.CurrentUser.Id != assessmentInput.ExaminerId)
            {
                request.ModelState.AddModelError(String.Empty, "Cannot appoint other users as examiners");
            }

            if (request.ModelState.IsValid)
            {
                var currentUser = request.CurrentUser;

                var a = await context.SkillAssessment.SingleOrDefaultAsync(e => e.Id == assessmentInput.Id);

                if (a == null)
                {
                    // validation for create
                    if (false == technology.Experts.Any(u => u.UserId == currentUser.Id))
                    {
                        request.ModelState.AddModelError(String.Empty, "Only " + technology.Name + "expert can assign skill assessment");
                    }
                    else if (assessmentInput.StudentId == currentUser.Id)
                    {
                        request.ModelState.AddModelError(String.Empty, "Cannot assign skill assessment to self");
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
                        request.ModelState.AddModelError(String.Empty, "Event has already happened");
                    }

                    if (a.ExaminerId != assessmentInput.ExaminerId ||
                        a.StudentId != assessmentInput.StudentId ||
                        a.TechnologyId != assessmentInput.TechnologyId)
                    {
                        request.ModelState.AddModelError(String.Empty, "Invalid assessment details");
                    }
                }

                // commit changes
                if (request.ModelState.IsValid)
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

                    return this.Response(new AssessmentInputVm { Id = a.Id });
                }
            }

            // restoring view model displayed properties

            assessmentInput.TechnologyName = technology?.Name;

            var student = await this.context.Users.SingleOrDefaultAsync(u => u.Id == assessmentInput.StudentId);
            assessmentInput.StudentName = student?.UserName;

            var examiner = await this.context.Users.SingleOrDefaultAsync(u => u.Id == assessmentInput.ExaminerId);
            assessmentInput.ExaminerName = examiner?.UserName;

            return this.ValidationError(assessmentInput);
        }

        /// <summary>
        /// Gets <see cref="SkillAssessment"/> details for display
        /// </summary>
        public async Task<Response<AssessmentDetailsVm>> DetailsGet(Request<int> request)
        {
            var details = await GetAssessmentDetails(request.CurrentUser.Id, request.Item);

            if (details == null)
            {
                return this.NotFound<AssessmentDetailsVm>();
            }

            return this.Response(details);
        }

        /// <summary>
        /// Gets <see cref="SkillAssessment"/> for marking by examiner 
        /// </summary>
        public async Task<Response<AssessmentDetailsVm>> MarkGet(Request<int> request)
        {
            var data = await GetAssessmentDetails(request.CurrentUser.Id, request.Item);

            if (data == null || false == data.CanMark)
            {
                return this.NotFound<AssessmentDetailsVm>();
            }

            return this.Response(data);
        }

        /// <summary>
        /// Applies <see cref="SkillAssessment"/> mark results
        /// </summary>
        public async Task<Response<AssessmentDetailsVm>> MarkPost(Request<AssessmentDetailsVm> request)
        {
            var vm = request.Item;

            var assessment = await this.GetAssessment(vm.Id);
            if (assessment == null)
            {
                return this.NotFound<AssessmentDetailsVm>();
            }

            if (assessment.IsPassed.HasValue)
            {
                request.ModelState.AddModelError(String.Empty, "Assessment has already been marked");
            }

            var user = request.CurrentUser;
            if (assessment.ExaminerId != user.Id)
            {
                request.ModelState.AddModelError(String.Empty, "Only examiner can mark assessment");
            }

            if (request.ModelState.IsValid && request.Item.IsPassed.HasValue)
            {
                assessment.ExamScore = vm.ExamScore;
                assessment.ExamComment = vm.ExamComment;
                assessment.IsPassed = vm.IsPassed;

                if (assessment.IsPassed.HasValue)
                {
                    this.context.Update(assessment);
                    await this.context.SaveChangesAsync();

                    return this.Response(new AssessmentDetailsVm { Id = assessment.Id });
                }
            }

            var a = await GetAssessment(assessment.Id);
            vm.Student = a.Student;
            vm.Examiner = a.Examiner;
            vm.Technology = a.Technology;
            vm.ExamDate = a.ExamDate;

            return this.ValidationError(vm);
        }

        /// <summary>
        /// Gets <see cref="SkillAssessment"/> for preview before deletion
        /// </summary>
        public async Task<Response<AssessmentDetailsVm>> DeleteGet(Request<int> request)
        {
            var data = await GetAssessmentDetails(request.CurrentUser.Id, request.Item);

            if (data == null || false == data.CanDelete)
            {
                return this.NotFound<AssessmentDetailsVm>();
            }

            return this.Response(data);
        }

        /// <summary>
        /// Deletes <see cref="SkillAssessment"/>
        /// </summary>
        public async Task<Response<AssessmentDetailsVm>> DeletePost(Request<int> request)
        {
            var data = await GetAssessmentDetails(request.CurrentUser.Id, request.Item);

            if (data == null)
            {
                return this.NotFound<AssessmentDetailsVm>();
            }

            if (false == data.CanDelete)
            {
                request.ModelState.AddModelError(String.Empty, "You cannot delete this record");
                return this.ValidationError(data);
            }

            var assessment = await context.SkillAssessment.SingleOrDefaultAsync(m => m.Id == request.Item);
            context.Remove(assessment);
            await context.SaveChangesAsync();

            return this.Response(data);
        }
    }
}
