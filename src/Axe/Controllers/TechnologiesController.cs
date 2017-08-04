using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Axe.Models;
using Axe.Models.TechnologiesVm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Axe.Controllers
{
    [Authorize]
    public class TechnologiesController : ControllerExt
    {
        public TechnologiesController(UserManager<ApplicationUser> userManager, AxeDbContext context) : base(userManager, context) { } 

        // GET: Technologies
        public async Task<IActionResult> Index(int? technologyId = null)
        {
            var user = await GetCurrentUserAsync();
            // get currrent user with list of technologies where they are an expert
            user = await this.context.Users.Include(u => u.Technologies).ThenInclude(x => x.Technology)
                             .SingleAsync(u => u.Id == user.Id);

            // create tech list for selection
            var techs = user.Technologies.Select(x => x.Technology).ToList();

            var selectedTech = techs.FirstOrDefault(t => t.Id == technologyId) ??
                               techs.FirstOrDefault();
            technologyId = selectedTech?.Id;

            // prepare list of users to assign as experts in selected technology
            var experts = new List<ExpertSelectionVm>();
            if (selectedTech != null)
            {
                selectedTech = this.context.Technology.Include(t => t.Experts)
                                   .SingleOrDefault(t => t.Id == technologyId);

                var expertsIds = selectedTech.Experts.Select(u => u.UserId).ToList();
                experts = this.context.Users
                              .Where(u => u.Id != user.Id)
                               .Select(u => new ExpertSelectionVm
                               {
                                   Id = u.Id,
                                   UserName = u.UserName,
                                   Email = u.Email,
                                   IsExpert = expertsIds.Contains(u.Id),
                               })
                               .ToList();
            }            

            var vm = new TechnologiesIndexVm
            {
                Technologies = techs,
                SelectedTechnology = selectedTech,

                Questions = this.context.TaskQuestion
                                .Include(q => q.Author)
                                .Where(q => q.TechnologyId == technologyId)
                                .ToList(),

                Exams = this.context.ExamTask.Include(t => t.Author)
                                .Include(t => t.Questions)
                                .Where(t => t.TechnologyId == technologyId)
                                .ToList(),

                Experts = experts
            };
            return View(vm);
        }

        // GET: Technologies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Technologies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,InformationText")] Technology technology)
        {
            if (ModelState.IsValid)
            {
                if (this.context.Technology.Any(t => t.Name == technology.Name))
                {
                    ModelState.AddModelError(String.Empty, technology.Name + " technology is already exists");
                }
            }

            if (ModelState.IsValid)
            {
                var user = await GetCurrentUserAsync();
                technology.Experts = new List<ExpertTechnologyLink> { new ExpertTechnologyLink { User = user, Technology = technology } };

                this.context.Add(technology);
                await this.context.SaveChangesAsync();
                return RedirectToAction("Index", new { technologyId = technology.Id });
            }
            return View(technology);
        }

        // GET: Technologies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Create");
            }

            var technology = await this.context.Technology.Include(t => t.Experts)
                                       .SingleOrDefaultAsync(m => m.Id == id);
            
            if (technology == null)
            {
                return NotFound();
            }

            var user = await GetCurrentUserAsync();

            // if User is not an Expert they cannot edit Technology
            if ( false == technology.Experts.Any(t=>t.UserId == user.Id))
            {
                return NotFound();
            }
            return View(technology);
        }

        // POST: Technologies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,InformationText")] Technology technologyInput)
        {
            if (id != technologyInput.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var technology = await this.context.Technology.Include(t => t.Experts)
                                               .SingleOrDefaultAsync(m => m.Id == id);

                    var user = await GetCurrentUserAsync();

                    // if User is not an Expert they cannot edit Technology
                    if (false == technology.Experts.Any(t => t.UserId == user.Id))
                    {
                        ModelState.AddModelError(String.Empty, "Only experts cna edit technologies");
                        return View(technologyInput);
                    }

                    this.context.Update(technologyInput);
                    await this.context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (false == TechnologyExists(technologyInput.Id))
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
            return View(technologyInput);
        }

        public async Task<IActionResult> IncludeExpert(string id, int technologyId)
        {
            await SetExpert(id, technologyId, true);
            return RedirectToAction("Index", new { technologyId });
        }

        public async Task<IActionResult> ExcludeExpert(string id, int technologyId)
        {
            await SetExpert(id, technologyId, false);
            return RedirectToAction("Index", new { technologyId });
        }

        private async Task SetExpert(string id, int technologyId, bool include)
        {
            var tech = await this.context.Technology.Include(t => t.Experts).SingleOrDefaultAsync(t => t.Id == technologyId);
            if (tech == null)
            {
                // requested tech is not found
                return;
            }

            var currentUser = await GetCurrentUserAsync();
            if (false == tech.Experts.Any(x => x.UserId == currentUser.Id))
            {
                // current user is not an expert in selected tech
                return;
            }

            if (false == this.context.Users.Any(u=>u.Id == id))
            {
                // requested user is not found
                return;
            }
            
            if (include && false == tech.Experts.Any(u => u.UserId == id))
            {
                // include selected User in Experts in requested Technology
                this.context.Add(new ExpertTechnologyLink { UserId = id, TechnologyId = technologyId });
                this.context.SaveChanges();
            }
            else if (false == include)
            {
                var expert = tech.Experts.FirstOrDefault(t => t.UserId == id);
                if (expert != null)
                {
                    // exclude selected User from Experts in requested Technology
                    this.context.Remove(expert);
                    await this.context.SaveChangesAsync();
                }
            }
        }

        // GET: Technologies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var technology = await this.context.Technology
                .SingleOrDefaultAsync(m => m.Id == id);
            if (technology == null)
            {
                return NotFound();
            }

            return View(technology);
        }

        // POST: Technologies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var technology = await this.context.Technology.SingleOrDefaultAsync(m => m.Id == id);
            this.context.Technology.Remove(technology);
            await this.context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool TechnologyExists(int id)
        {
            return this.context.Technology.Any(e => e.Id == id);
        }
    }
}
