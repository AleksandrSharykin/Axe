using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Axe.Models;
using Microsoft.AspNetCore.Identity;

namespace Axe.Controllers
{
    public class ControllerExt : Controller
    {
        protected UserManager<ApplicationUser> userManager;
        protected AxeDbContext context;

        public ControllerExt(UserManager<ApplicationUser> userManager, AxeDbContext context)
        {
            this.userManager = userManager;
            this.context = context;
        }

        protected Task<ApplicationUser> GetCurrentUserAsync()
        {

            return this.userManager.GetUserAsync(HttpContext.User);
        }

        protected async Task<Request<T>> CreateRequest<T>(T item)
        {
            return new Request<T>(item)
            {
                CurrentUser = await this.GetCurrentUserAsync(),
                ModelState = this.ModelState,
            };
        }
    }
}
