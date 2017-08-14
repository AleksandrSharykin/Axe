using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Axe.Models;
using Axe.Managers;

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
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

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
