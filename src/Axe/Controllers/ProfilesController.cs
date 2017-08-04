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
using Axe.Models.ProfileViewModels;

namespace Axe.Controllers
{
    [Authorize]
    public class ProfilesController : ControllerExt
    {        
        private readonly SignInManager<ApplicationUser> _signInManager;        
        private readonly string _externalCookieScheme;
        private readonly ILogger _logger;

        public ProfilesController(
          UserManager<ApplicationUser> userManager,
          SignInManager<ApplicationUser> signInManager,
          AxeDbContext context,
          IOptions<IdentityCookieOptions> identityCookieOptions,
          ILoggerFactory loggerFactory)
            : base (userManager, context)
        {
            this.userManager = userManager;
            _signInManager = signInManager;
            this.context = context;
            _externalCookieScheme = identityCookieOptions.Value.ExternalCookieAuthenticationScheme;
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
            //ViewData["StatusMessage"] =
            //    message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
            //    : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
            //    : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
            //    : message == ManageMessageId.Error ? "An error has occurred."
            //    : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
            //    : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
            //    : "";

            ApplicationUser user = await GetCurrentUserAsync();

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

                attempts = await this.context.ExamAttempt.Where(t => t.TechnologyId == selectedTech.Id).ToListAsync();

                bestAttempts = attempts.GroupBy(a => a.Task.Id)
                                       .Select(g => g.OrderByDescending(a => a.ExamScore).First())
                                       .ToList();
            }

            bool self = profile.Id == user.Id;
            var model = new ProfileInfoVm
            {
                Id = profile.Id,
                Self = self,
                UserName = profile.UserName,
                JobPosition = profile.JobPosition,
                ContactInfo = profile.Email,

                Technologies = techs,
                SelectedTechnology = selectedTech,

                ExpertKnowledge = profile.Technologies.Select(t => t.Technology).ToList(),

                Skills = profile.GetSkills().Concat(profile.AssessmentsAsStudent.Where(a => a.ExamScore is null)).ToList(),
                Assessments = profile.AssessmentsAsStudent.Where(a=>a.TechnologyId == selectedTech.Id),

                                AllAttempts = attempts ?? new List<ExamAttempt>(),
                BestAttempts = bestAttempts ?? new List<ExamAttempt>(),
                Tasks = self ? ( tasks ?? new List<ExamTask>() ) : null,
            };

            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel account)
        {
            ManageMessageId? message = ManageMessageId.Error;
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await this.userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    message = ManageMessageId.RemoveLoginSuccess;
                }
            }
            return RedirectToAction(nameof(ManageLogins), new { Message = message });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableTwoFactorAuthentication()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                await this.userManager.SetTwoFactorEnabledAsync(user, true);
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(1, "User enabled two-factor authentication.");
            }
            return RedirectToAction(nameof(Index), "Profiles");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableTwoFactorAuthentication()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                await this.userManager.SetTwoFactorEnabledAsync(user, false);
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(2, "User disabled two-factor authentication.");
            }
            return RedirectToAction(nameof(Index), "Profiles");
        }



        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction(nameof(Index));

            return View(new EditProfileVm { UserName = user.UserName, JobPosition = user.JobPosition });
        }

        public async Task<IActionResult> Edit(EditProfileVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction(nameof(Index));

            user.UserName = model.UserName;
            user.JobPosition = model.JobPosition;
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
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await GetCurrentUserAsync();
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
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();
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

        //GET: /Manage/ManageLogins
        [HttpGet]
        public async Task<IActionResult> ManageLogins(ManageMessageId? message = null)
        {
            ViewData["StatusMessage"] =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.AddLoginSuccess ? "The external login was added."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await this.userManager.GetLoginsAsync(user);
            var otherLogins = _signInManager.GetExternalAuthenticationSchemes().Where(auth => userLogins.All(ul => auth.AuthenticationScheme != ul.LoginProvider)).ToList();
            ViewData["ShowRemoveButton"] = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkLogin(string provider)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.Authentication.SignOutAsync(_externalCookieScheme);

            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Action(nameof(LinkLoginCallback), "Profiles");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, this.userManager.GetUserId(User));
            return Challenge(properties, provider);
        }

        //
        // GET: /Manage/LinkLoginCallback
        [HttpGet]
        public async Task<ActionResult> LinkLoginCallback()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var info = await _signInManager.GetExternalLoginInfoAsync(await this.userManager.GetUserIdAsync(user));
            if (info == null)
            {
                return RedirectToAction(nameof(ManageLogins), new { Message = ManageMessageId.Error });
            }
            var result = await this.userManager.AddLoginAsync(user, info);
            var message = ManageMessageId.Error;
            if (result.Succeeded)
            {
                message = ManageMessageId.AddLoginSuccess;
                // Clear the existing external cookie to ensure a clean login process
                await HttpContext.Authentication.SignOutAsync(_externalCookieScheme);
            }
            return RedirectToAction(nameof(ManageLogins), new { Message = message });
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
