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
                string template =
                @"using System;
                namespace Axe
                {
                    public class AxeTask
                    {
                        public int Main()
                        {
                
                        }
                    }
                }";
                CodeBlock codeBlock = context.CodeBlock.First(c => c.Id == id);
                CodeBlockVm model = new CodeBlockVm
                {
                    Id = codeBlock.Id,
                    Task = codeBlock.Task,
                    SourceCode = CSharpSyntaxTree.ParseText(template).GetRoot().NormalizeWhitespace().ToFullString()
                };
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
            //System.Threading.Thread.Sleep(3000);
            if (ModelState.IsValid)
            {
                try
                {
                    CodeBlock codeBlock = context.CodeBlock.Include(cb => cb.TestCases).First(cb => cb.Id == model.Id);

                    string assemblyName = "AxeCompilerAssembly" + Path.GetRandomFileName();
                    // Random file name is necessary because .NETCore doesn't allow to unload assembly
                    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(model.SourceCode);
                    model.SourceCode = syntaxTree.GetRoot().NormalizeWhitespace().ToFullString();

                    StatementSyntax syntaxOfVerificationCode = SyntaxFactory.ParseStatement(codeBlock.VerificationCode);
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
        public IActionResult Create(CodeBlockTaskVm model)
        {
            if (ModelState.IsValid)
            {
                CodeBlock codeBlock = new CodeBlock();
                codeBlock.Task = model.Task;
                foreach (var testCase in model.TestCases)
                {
                    codeBlock.TestCases.Add(new TestCaseCodeBlock { Input = testCase.Input, Output = testCase.Output });
                }
                codeBlock.OutputType = model.OutputType;

                codeBlock.VerificationCode +=
                    @"bool[] resultsOfTestCases_#s# = new bool[#TEST_CASE_COUNT#];
                    for (int i_#s# = 0; i_#s# < resultsOfTestCases_#s#.Length; i_#s#++)                    
                        resultsOfTestCases_#s#[i_#s#] = true;";
                for (int i = 0; i < codeBlock.TestCases.Count; i++)
                {
                    codeBlock.VerificationCode +=
                        @"#TYPE_FUNC# result_#s#_#INDEX# = Main(#INPUT#);";
                    switch (codeBlock.OutputType)
                    {
                        case OutputTypeEnum.INT:
                        case OutputTypeEnum.DOUBLE:
                        case OutputTypeEnum.STRING:
                            {
                                codeBlock.VerificationCode +=
                                @"if (result_#s#_#INDEX# != #OUTPUT#)
                                    resultsOfTestCases_#s#[#INDEX#] = false;";
                                break;
                            }
                        case OutputTypeEnum.INT_ARRAY:
                        case OutputTypeEnum.DOUBLE_ARRAY:
                        case OutputTypeEnum.STRING_ARRAY:
                            {
                                codeBlock.VerificationCode +=
                                @"var array_#s#_#INDEX# = #OUTPUT#;
                                if (array_#s#_#INDEX#.Length != result_#s#_#INDEX#.Length)
                                    resultsOfTestCases_#s#[#INDEX#] = false;
                                else
                                    for (int i_#s# = 0; i_#s# < array_#s#_#INDEX#.Length; i_#s#++)
                                    {
                                        if (array_#s#_#INDEX#[i_#s#] != result_#s#_#INDEX#[i_#s#])
                                        resultsOfTestCases_#s#[#INDEX#] = false;
                                    }";
                                break;
                            }
                        default:
                            break;
                    }

                    codeBlock.VerificationCode = codeBlock.VerificationCode
                        .Replace("#INDEX#", i.ToString())
                        .Replace("#OUTPUT#", codeBlock.TestCases[i].Output)
                        .Replace("#INPUT#", codeBlock.TestCases[i].Input);
                }

                switch (codeBlock.OutputType)
                {
                    case OutputTypeEnum.INT:
                        {
                            codeBlock.VerificationCode = codeBlock.VerificationCode
                                .Replace("#TYPE_FUNC#", "int");
                            break;
                        }
                    case OutputTypeEnum.DOUBLE:
                        {
                            codeBlock.VerificationCode = codeBlock.VerificationCode
                                .Replace("#TYPE_FUNC#", "double");
                            break;
                        }
                    case OutputTypeEnum.STRING:
                        {
                            codeBlock.VerificationCode = codeBlock.VerificationCode
                                .Replace("#TYPE_FUNC#", "string");
                            break;
                        }
                    case OutputTypeEnum.INT_ARRAY:
                        {
                            codeBlock.VerificationCode = codeBlock.VerificationCode
                                .Replace("#TYPE_FUNC#", "int[]");
                            break;
                        }
                    case OutputTypeEnum.DOUBLE_ARRAY:
                        {
                            codeBlock.VerificationCode = codeBlock.VerificationCode
                                .Replace("#TYPE_FUNC#", "double[]");
                            break;
                        }
                    case OutputTypeEnum.STRING_ARRAY:
                        {
                            codeBlock.VerificationCode = codeBlock.VerificationCode
                                .Replace("#TYPE_FUNC#", "string[]");
                            break;
                        }
                    default:
                        break;
                }

                codeBlock.VerificationCode +=
                @"for (int i_#s# = 0; i_#s# < resultsOfTestCases_#s#.Length; i_#s#++)
                {
                    if (!resultsOfTestCases_#s#[i_#s#]) return false;
                }
                return true;";

                codeBlock.VerificationCode = codeBlock.VerificationCode
                    .Replace("#TEST_CASE_COUNT#", codeBlock.TestCases.Count.ToString())
                    .Replace("#s#", "AXE");

                context.CodeBlock.Add(codeBlock);
                context.SaveChanges();
            }
            return View(model);
        }
    }
}