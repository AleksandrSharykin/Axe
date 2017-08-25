using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Axe.Models;
using Axe.Managers;
using Microsoft.AspNetCore.Authorization;

namespace Axe.Controllers
{
    public class HomeController : ControllerExt
    {
        private IHomeManager manager;

        public HomeController(UserManager<ApplicationUser> userManager, IHomeManager manager) : base(userManager, null)
        {
            this.manager = manager;
        }

        public async Task<IActionResult> Index()
        {
            var response = await this.manager.Index();
            return View(response.Item);
        }

        public IActionResult About()
        {
            return View();
        }

        public async Task<IActionResult> Contact()
        {
            var request = await this.CreateRequest(0);

            var response = await this.manager.GetAdministrators(request);
            if (response.Item.Count > 0)
            {
                ViewData["Admins"] = response.Item;
            }

            return View();
        }

        [Authorize(Roles = "superuser")]
        public IActionResult Statistics()
        {
            return View();
        }

        public IActionResult Error(string id = null)
        {
            if (id == "404")
            {
                return View("NotFound");
            }

            return View();
        }
    }
}
