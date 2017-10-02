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

namespace Axe.Controllers
{
    public class CompilerController : ControllerExt
    {
        private ICompilerManager compileManager;
        private ITechnologyManager technologyManager;
        private INodeServices nodeServices;

        public CompilerController(UserManager<ApplicationUser> userManager,
            ICompilerManager compileManager,
            ITechnologyManager technologyManager,
            INodeServices nodeServices,
            AxeDbContext context): base(userManager, context)
        {
            this.compileManager = compileManager;
            this.technologyManager = technologyManager;
            this.nodeServices = nodeServices;
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
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Solve(CodeBlockSolveVm model)
        {
            model.ListOfTechnologies = new SelectList(await technologyManager.GetTechnologies(), "Id", "Name");
            model.Technology = await technologyManager.GetTechnologyById(model.SelectedTechnologyId);
            model.SourceCode = compileManager.FormatCode(model.SourceCode);
            if (ModelState.IsValid)
            {
                try
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
                catch (Exception exception)
                {
                    ModelState.AddModelError("", exception.Message);
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            CodeBlockCreateVm model = new CodeBlockCreateVm();
            model.ListOfTechnologies = new SelectList(await technologyManager.GetTechnologies(), "Id", "Name");
            model.TestCases.Add(new TestCaseCodeBlock());
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CodeBlockCreateVm model)
        {
            model.ListOfTechnologies = new SelectList(await technologyManager.GetTechnologies(), "Id", "Name");
            if (ModelState.IsValid)
            {
                try
                {
                    await compileManager.Create(model);
                    return RedirectToAction(nameof(CompilerController.Index));
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError("", exception.Message);
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                CodeBlockCreateVm model = await compileManager.GetByIdForEdit(id);
                model.ListOfTechnologies = new SelectList(await technologyManager.GetTechnologies(), "Id", "Name");
                return View(model);
            }
            catch (Exception exception)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CodeBlockCreateVm model)
        {
            model.ListOfTechnologies = new SelectList(await technologyManager.GetTechnologies(), "Id", "Name");
            if (ModelState.IsValid)
            {
                try
                {
                    await compileManager.Update(model);
                    return RedirectToActionPermanent(nameof(CompilerController.Index));
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError("Task", exception.Message);
                    return View(model);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                CodeBlockSolveVm model = await compileManager.GetCodeBlockById(id);
                return View(model);
            }
            catch (Exception exception)
            {
                return NotFound();
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await compileManager.DeleteById(id);
                return RedirectToActionPermanent(nameof(CompilerController.Index));
            }
            catch (Exception exception)
            {
                return NotFound();
            }
        }
    }
}