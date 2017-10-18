using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Axe.Models;

namespace Axe.Controllers
{
    [Produces("application/json")]
    [Route("api/users/[action]")]
    public class UsersApiController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;

        public UsersApiController(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        [HttpPost]
        public async Task<ApplicationUser> GetUserById(string id)
        {
            return await userManager.FindByIdAsync(id);
        }
    }
}
