using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Axe.Models;
using Axe.Models.ProfilesVm;

namespace Axe.Controllers
{
    [Authorize]
    public class ProfilesController : ControllerExt
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;

        public ProfilesController(
          UserManager<ApplicationUser> userManager,
          SignInManager<ApplicationUser> signInManager,
          AxeDbContext context,
          ILoggerFactory loggerFactory)
            : base(userManager, context)
        {
            this.userManager = userManager;
            _signInManager = signInManager;
            this.context = context;
            _logger = loggerFactory.CreateLogger<ProfilesController>();
        }

        /// <summary>
        /// Return list of all registered users (possibly filtered)
        /// </summary>
        /// <param name="userFilter">Seacrh filter to match UserNames</param>
        /// <returns></returns>
        public async Task<IActionResult> Index(string userFilter = null)
        {
            IQueryable<ApplicationUser> userList = context.Users
                .Include(u => u.AssessmentsAsStudent).ThenInclude(a => a.Technology)
                .Include(u => u.Technologies).ThenInclude(t => t.Technology);

            if (false == String.IsNullOrWhiteSpace(userFilter))
                userList = userList.Where(u => u.UserName.Contains(userFilter) || u.Email.Contains(userFilter) || u.JobPosition.Contains(userFilter));

            ViewData[nameof(userFilter)] = userFilter;

            var data = (await userList.ToListAsync())
                .Select(u => new IndexVm
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    ContactInfo = u.Email,
                    JobPosition = u.JobPosition,
                    Skills = u.GetSkills().ToList(),
                    ExpertKnowledge = u.Technologies.Select(t => t.Technology).ToList(),
                }).ToList();

            return View(data);
        }

        /// <summary>
        /// Returns data for user profile
        /// </summary>
        /// <param name="id"></param>
        /// <param name="technologyId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Visit(string id = null, int? technologyId = null)
        {
            ApplicationUser user = await this.GetCurrentUserAsync();

            var users = this.context.Users
                .Include(u => u.AssessmentsAsStudent).ThenInclude(a => a.Examiner)
                .Include(u => u.Technologies).ThenInclude(t => t.Technology);
            ApplicationUser profile = await users.SingleOrDefaultAsync(u => u.Id == id) ??
                                      await users.SingleOrDefaultAsync(u => u.Id == user.Id);

            if (profile == null)
            {
                return View("Error");
            }

            var techs = await this.context.Technology.ToListAsync();
            var selectedTech = techs.FirstOrDefault(t => t.Id == technologyId) ??
                               techs.FirstOrDefault();

            IList<ExamTask> tasks = null;
            IList<ExamAttempt> attempts = null;
            IList<ExamAttempt> bestAttempts = null;

            if (selectedTech != null)
            {
                tasks = await this.context.ExamTask.Where(t => t.TechnologyId == selectedTech.Id).ToListAsync();

                attempts = await this.context.ExamAttempt.Where(a => a.TechnologyId == selectedTech.Id && a.StudentId == profile.Id).ToListAsync();

                bestAttempts = attempts.Where(a => a.IsFinished)
                                       .GroupBy(a => a.Task.Id)
                                       .Select(g => g.OrderByDescending(a => a.ExamScore).First())
                                       .ToList();
            }

            bool self = profile.Id == user.Id;
            var model = new ProfileDetailsVm
            {
                Id = profile.Id,
                Self = self,
                UserName = profile.UserName,
                JobPosition = profile.JobPosition,
                ContactInfo = profile.Email,

                Technologies = techs,
                SelectedTechnology = selectedTech,

                ExpertKnowledge = profile.Technologies.Select(t => t.Technology).ToList(),

                Skills = profile.GetSkills().Concat(profile.AssessmentsAsStudent.Where(a => a.IsPassed is null)).ToList(),
                Assessments = profile.AssessmentsAsStudent.Where(a => a.TechnologyId == selectedTech.Id),

                AllAttempts = attempts ?? new List<ExamAttempt>(),
                BestAttempts = bestAttempts ?? new List<ExamAttempt>(),
                Tasks = self ? (tasks ?? new List<ExamTask>()) : null,
            };

            return View(model);
        }

        /// <summary>
        /// Loads user avatar from db
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns></returns>
        public ActionResult GetAvatar(string id)
        {
            byte[] imageData = this.context.Users.Where(u => u.Id == id).Select(u => u.Avatar).FirstOrDefault();
            if (imageData != null)
            {
                return File(imageData, "image/png");
            }
            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await this.GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction(nameof(Index));

            return View(new EditProfileVm { Id = user.Id, UserName = user.UserName, JobPosition = user.JobPosition });
        }

        public async Task<IActionResult> Edit(EditProfileVm model)
        {
            if (false == ModelState.IsValid)
            {
                return View(model);
            }

            var user = await this.GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction(nameof(Index));

            user.UserName = model.UserName;
            user.JobPosition = model.JobPosition;
            if (model.AvatarImage != null)
            {
                using (var memoryStream = new System.IO.MemoryStream())
                {
                    await model.AvatarImage.CopyToAsync(memoryStream);
                    user.Avatar = memoryStream.ToArray();
                }
            }
            this.context.Update(user);
            this.context.SaveChanges();

            return RedirectToAction(nameof(Visit));
        }

        //
        // GET: /Manage/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await this.GetCurrentUserAsync();
            if (user != null)
            {
                var result = await this.userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, "User changed their password successfully.");
                    return RedirectToAction(nameof(Visit), new { Message = ManageMessageId.ChangePasswordSuccess });
                }
                AddErrors(result);
                return View(model);
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        //
        // GET: /Manage/SetPassword
        [HttpGet]
        public IActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await this.GetCurrentUserAsync();
            if (user != null)
            {
                var result = await this.userManager.AddPasswordAsync(user, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
                return View(model);
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            AddLoginSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

        #endregion
    }
}
