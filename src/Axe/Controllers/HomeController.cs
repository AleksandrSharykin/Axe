using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Axe.Models;
using Microsoft.AspNetCore.Identity;

namespace Axe.Controllers
{
    public class HomeController : ControllerExt
    {
        public HomeController(UserManager<ApplicationUser> userManager, AxeDbContext context) : base(userManager, context) { }                

        public async Task<IActionResult> Index()
        {
            return View(await this.context.Technology.ToListAsync());
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

        public IActionResult Error()
        {
            return View();
        }
    }
}
