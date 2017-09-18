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

namespace Axe.Controllers
{
    public class CompilerController : ControllerExt
    {
        private ICompileManager compileManager;

        public CompilerController(UserManager<ApplicationUser> userManager, ICompileManager compileManager, AxeDbContext context)
            : base(userManager, context)
        {
            this.compileManager = compileManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<CodeBlockVm> model = await compileManager.GetCodeBlocks();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Solve(int id)
        {
            try
            {
                CodeBlockVm model = await compileManager.GetById(id);
                return View(model);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public IActionResult Solve(CodeBlockVm model)
        {
            System.Threading.Thread.Sleep(2000);
            if (ModelState.IsValid)
            {
                try
                {
                    model.SourceCode = CSharpSyntaxTree.ParseText(model.SourceCode).GetRoot().NormalizeWhitespace().ToFullString();
                    Tuple<CodeBlockResult, string[]> result = compileManager.Solve(model);
                    ViewData["StatusOfExecuting"] = result.Item1;
                    if (result.Item1 == CodeBlockResult.ERROR)
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
        public IActionResult Create()
        {
            CodeBlockTaskVm model = new CodeBlockTaskVm();
            model.TestCases.Add(new TestCaseCodeBlock());
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CodeBlockTaskVm model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await compileManager.Create(model);
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError("", exception.Message);
                    return View(model);
                }
            }
            return View(model);
        }

    }
}