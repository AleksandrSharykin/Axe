using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Axe.Models;
using Axe.Models.TechnologiesVm;

namespace Axe.Controllers
{
    public class TechnologiesController : Controller
    {
        private readonly AxeDbContext context;

        public TechnologiesController(AxeDbContext context)
        {
            this.context = context;    
        }

        // GET: Technologies
        public async Task<IActionResult> Index(int? technologyId = null)
        {
            var techs = await this.context.Technology.ToListAsync();
            var selectedTech = techs.FirstOrDefault(t => t.Id == technologyId) ??
                               techs.FirstOrDefault();
            technologyId = selectedTech?.Id;

            var vm = new TechnologiesIndexVm
            {
                Technologies = techs,
                SelectedTechnology = selectedTech,
                Questions = this.context.TaskQuestion.Include(q => q.Author)
                                .Where(q => q.TechnologyId == technologyId)
                                .ToList(),
                Exams = this.context.ExamTask.Include(t => t.Author).Include(t=>t.Questions)
                            .Where(t => t.TechnologyId == technologyId)
                            .ToList(),
            };
            return View(vm);
        }

        // GET: Technologies/Details/5
        public async Task<IActionResult> Details(int? id)
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
                this.context.Add(technology);
                await this.context.SaveChangesAsync();
                return RedirectToAction("Index");
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

            var technology = await this.context.Technology.SingleOrDefaultAsync(m => m.Id == id);
            if (technology == null)
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,InformationText")] Technology technology)
        {
            if (id != technology.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    this.context.Update(technology);
                    await this.context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TechnologyExists(technology.Id))
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
            return View(technology);
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
