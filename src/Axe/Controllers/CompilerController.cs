using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Axe.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Axe.ViewModels.CompilerVm;
using Microsoft.EntityFrameworkCore;
using Axe.Managers;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace Axe.Controllers
{
    public class CompilerController : ControllerExt
    {
        private ICompilerManager compileManager;
        private ITechnologyManager technologyManager;

        public CompilerController(UserManager<ApplicationUser> userManager,
            ICompilerManager compileManager,
            ITechnologyManager technologyManager,
            AxeDbContext context): base(userManager, context)
        {
            this.compileManager = compileManager;
            this.technologyManager = technologyManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CodeBlockIndexVm model)
        {         
            model.ListOfCodeBlocks = await compileManager.GetCodeBlocks(model.SelectedTechnologyId);
            model.ListOfTechnologies = new SelectList(await technologyManager.GetTechnologies(), "Id", "Name");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Solve(int id)
        {
            try
            {
                CodeBlockSolveVm model = await compileManager.GetCodeBlockById(id);
                model.ListOfTechnologies = new SelectList(await technologyManager.GetTechnologies(), "Id", "Name");
                return View(model);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Solve(CodeBlockSolveVm model)
        {
            try
            {
                model.ListOfTechnologies = new SelectList(await technologyManager.GetTechnologies(), "Id", "Name");
                model.Technology = await technologyManager.GetTechnologyById(model.SelectedTechnologyId);
                model.SourceCode = compileManager.FormatCode(model.SourceCode);
                if (ModelState.IsValid)
                {
                    Tuple<CodeBlockResult, string[]> result = await compileManager.HandleCodeBlock(model);
                    model.Result = result.Item1;
                    if (result.Item1 == CodeBlockResult.Error || result.Item1 == CodeBlockResult.Failed)
                    {
                        for (int i = 0; i < result.Item2.Length; i++)
                        {
                            ModelState.AddModelError(i.ToString(), result.Item2[i]);
                        }
                    }
                }
                return View(model);
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("Task", exception.Message);
                return View(model);
            }
        }

        [Authorize(Roles = "superuser")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            CodeBlockCreateVm model = new CodeBlockCreateVm();
            model.ListOfTechnologies = new SelectList(await technologyManager.GetTechnologies(), "Id", "Name");
            model.TestCases.Add(new TestCaseCodeBlock());
            return View(model);
        }

        [Authorize(Roles = "superuser")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CodeBlockCreateVm model)
        {
            try
            {
                model.ListOfTechnologies = new SelectList(await technologyManager.GetTechnologies(), "Id", "Name");
                if (ModelState.IsValid)
                {
                    await compileManager.Create(model);
                    return RedirectToAction(nameof(CompilerController.Index));
                }
                return View(model);
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("Task", exception.Message);
                return View(model);
            }
        }

        [Authorize(Roles = "superuser")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                CodeBlockCreateVm model = await compileManager.GetByIdForEdit(id);
                model.ListOfTechnologies = new SelectList(await technologyManager.GetTechnologies(), "Id", "Name");
                return View(model);
            }
            catch
            {
                return NotFound();
            }
        }

        [Authorize(Roles = "superuser")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CodeBlockCreateVm model)
        {
            try
            {
                model.ListOfTechnologies = new SelectList(await technologyManager.GetTechnologies(), "Id", "Name");
                if (ModelState.IsValid)
                {
                    await compileManager.Update(model);
                    return RedirectToActionPermanent(nameof(CompilerController.Index));
                }
                return View(model);
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("Task", exception.Message);
                return View(model);
            }
        }

        [Authorize(Roles = "superuser")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                CodeBlockSolveVm model = await compileManager.GetCodeBlockById(id);
                return View(model);
            }
            catch
            {
                return NotFound();
            }
        }

        [Authorize(Roles = "superuser")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await compileManager.DeleteById(id);
                return RedirectToActionPermanent(nameof(CompilerController.Index));
            }
            catch
            {
                return NotFound();
            }
        }
    }
}