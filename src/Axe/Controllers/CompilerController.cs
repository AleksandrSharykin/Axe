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
        public CompilerController(UserManager<ApplicationUser> userManager, AxeDbContext context)
            : base(userManager, context)
        {
        }

        public IActionResult Index()
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
                /*
                @"
                using System;
                namespace RoslynCompileSample
                {
                    public static class Program
                    {
                        public static void Main()
                        {
                            System.Console.WriteLine(""Hello"");
                        }
                    }
                }
                "
                */
                
                @"
                using System;
                namespace RoslynCompileSample
                {
                    public class Writer
                    {
                        public int Write(string message)
                        {
                            Console.WriteLine(message);
                            return 4;
                        }
                    }
                }"
                
                );
            
            string assemblyName = "AxeCompilerAssembly";

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
                }
                else
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    
                    AssemblyLoadContext context = AssemblyLoadContext.Default;
                    Assembly assembly = context.LoadFromStream(stream);                    
                    MethodInfo method = assembly.GetType("RoslynCompileSample.Writer").GetMethod("Write");
                    Object writerClass = assembly.CreateInstance("RoslynCompileSample.Writer");
                    Object output = method.Invoke(writerClass, new Object[] { "test_string" });
                    Console.WriteLine("Method returned {0}", output);
                }

            }

            return View();
        }
    }
}