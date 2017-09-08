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

namespace Axe.Controllers
{
    public class CompilerController : ControllerExt
    {
        private AxeDbContext _context;

        public CompilerController(UserManager<ApplicationUser> userManager, AxeDbContext context)
            : base(userManager, context)
        {
            this._context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<CodeBlock> codeBlocks = this._context.CodeBlock.ToList();
            return View(codeBlocks);
        }

        [HttpPost]
        public IActionResult Execute(CodeBlock codeBlock)
        {
            if (ModelState.IsValid)
            {
                if (codeBlock != null)
                {
                    string assemblyName = "AxeCompilerAssembly" + Path.GetRandomFileName();
                    // Random file name is necessary because .NETCore doesn't allow to unload assembly
                    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(codeBlock.Code);

                    MetadataReference[] references = new MetadataReference[]
                    {
                    MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Hashtable).GetTypeInfo().Assembly.Location)
                        // MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                        // MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
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
                                Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                            }
                            ViewData["StatusOfExecuting"] = "Error";
                        }
                        else
                        {
                            stream.Seek(0, SeekOrigin.Begin);

                            AssemblyLoadContext context = AssemblyLoadContext.Default;
                            Assembly assembly = context.LoadFromStream(stream);
                            MethodInfo method = assembly.GetType("RoslynCompileSample.Writer").GetMethod("Write");
                            Object writerClass = assembly.CreateInstance("RoslynCompileSample.Writer");
                            Object output = method.Invoke(writerClass, new Object[] { new int[] { 6, 3, 1, 59334, 232, 3, -1 } });

                            Console.WriteLine("Method returned {0}", output);
                            CodeBlock codeBlock1 = this.context.CodeBlock.First(cb => cb.Id == codeBlock.Id);
                            var answer = (codeBlock1.Output == output.ToString());

                            ViewData["StatusOfExecuting"] = answer ? "Ok" : "Output isn't correct";
                        }
                    }
                }
            }
            return View("Index", null);
        }
    }
}