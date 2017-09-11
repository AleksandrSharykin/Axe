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

namespace Axe.Controllers
{
    public class CompilerController : ControllerExt
    {
        public CompilerController(UserManager<ApplicationUser> userManager, AxeDbContext context)
            : base(userManager, context)
        {
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<CodeBlock> codeBlocks = this.context.CodeBlock.ToList();
            return View(codeBlocks);
        }

        [HttpGet]
        public IActionResult Solve(int id)
        {
            try
            {
                CodeBlock codeBlock = context.CodeBlock.First(c => c.Id == id);
                codeBlock.SourceCode = CSharpSyntaxTree.ParseText(codeBlock.SourceCode).GetRoot().NormalizeWhitespace().ToFullString(); // Source code formatting
                return View(codeBlock);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public IActionResult Solve(CodeBlock codeBlock)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string assemblyName = "AxeCompilerAssembly" + Path.GetRandomFileName();
                    // Random file name is necessary because .NETCore doesn't allow to unload assembly
                    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(codeBlock.SourceCode);

                    StatementSyntax syntaxOfVerificationCode = SyntaxFactory.ParseStatement(codeBlock.VerificationCode.Replace("#number#", "AXE"));
                    MethodDeclarationSyntax methodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("bool"), "CHECK")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithBody(SyntaxFactory.Block(syntaxOfVerificationCode));

                    ClassDeclarationSyntax axeTaskClass = syntaxTree.GetRoot().DescendantNodes()
                        .OfType<ClassDeclarationSyntax>()
                        .First(c => c.Identifier.ValueText == "AxeTask");

                    ClassDeclarationSyntax newAxeTaskClass = axeTaskClass.AddMembers(methodDeclaration);
                    SyntaxNode syntaxNode = syntaxTree.GetRoot().ReplaceNode(axeTaskClass, newAxeTaskClass);
                    string sourceCode = syntaxNode.NormalizeWhitespace().ToFullString();
                    syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

                    MetadataReference[] references = new MetadataReference[]
                    {
                        MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Hashtable).GetTypeInfo().Assembly.Location)
                    };

                    CSharpCompilation compilation = CSharpCompilation.Create(
                        assemblyName: assemblyName,
                        syntaxTrees: new[] { syntaxTree },
                        references: references,
                        options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                    using (var stream = new MemoryStream())
                    {
                        EmitResult result = compilation.Emit(stream);
                        if (!result.Success)
                        {
                            IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                                diagnostic.IsWarningAsError ||
                                diagnostic.Severity == DiagnosticSeverity.Error);

                            foreach (Diagnostic diagnostic in failures)
                            {
                                ModelState.AddModelError(diagnostic.Id, diagnostic.GetMessage());
                            }
                            ViewData["StatusOfExecuting"] = "error";
                        }
                        else
                        {
                            stream.Seek(0, SeekOrigin.Begin);

                            AssemblyLoadContext context = AssemblyLoadContext.Default;
                            Assembly assembly = context.LoadFromStream(stream);

                            MethodInfo method = assembly.GetType("Axe.AxeTask").GetMethod("CHECK");
                            Object axeClass = assembly.CreateInstance("Axe.AxeTask");
                            Object output = method.Invoke(axeClass, null);

                            bool isCorrect = (bool)output;
                            ViewData["StatusOfExecuting"] = isCorrect ? "ok" : "no";
                        }
                    }
                }
                catch (InvalidOperationException exception)
                {
                    ModelState.AddModelError("", exception.Message);
                    return View(codeBlock);
                }
            }
            return View(codeBlock);
        }
    }
}