using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Axe.Models;
using Axe.ViewModels.CompilerVm;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
using System.Collections;
using Microsoft.CodeAnalysis.Emit;
using System.Runtime.Loader;

namespace Axe.Managers
{
    /// <summary>
    /// Class implements operations which can be performed with <see cref="CodeBlock"/> entities
    /// </summary>
    public class CompileManager : ManagerBase, ICompileManager
    {
        public CompileManager(AxeDbContext context) : base(context)
        {
        }
        
        public async Task<List<CodeBlockVm>> GetCodeBlocks()
        {
            List<CodeBlockVm> list = await context.CodeBlock.Select(cb => new CodeBlockVm
            {
                Id = cb.Id,
                Task = cb.Task
            }).ToListAsync();
            return list;
        }

        public async Task<CodeBlockVm> GetById(int id)
        {
            try
            {
                CodeBlock codeBlock = await this.context.CodeBlock.FirstAsync(cb => cb.Id == id);
                CodeBlockVm model = new CodeBlockVm()
                {
                    Id = codeBlock.Id,
                    Task = codeBlock.Task
                };
                model.SourceCode = CSharpSyntaxTree.ParseText(model.SourceCode).GetRoot().NormalizeWhitespace().ToFullString();
                return model;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public Tuple<CodeBlockResult, string[]> Solve(CodeBlockVm model)
        {
            try
            {
                CodeBlock codeBlock = context.CodeBlock
                    .Include(cb => cb.TestCases)
                    .First(cb => cb.Id == model.Id);

                string assemblyName = "AxeCompilerAssembly" + Path.GetRandomFileName();
                // Random file name is necessary because .NETCore doesn't allow to unload assembly
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(model.SourceCode);

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
                        Diagnostic[] failures = result.Diagnostics.Where(diagnostic =>
                             diagnostic.IsWarningAsError ||
                             diagnostic.Severity == DiagnosticSeverity.Error).ToArray();

                        string[] errors = failures.Select(f => f.GetMessage()).ToArray();
                        return new Tuple<CodeBlockResult, string[]>(CodeBlockResult.ERROR, errors);
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
                        return new Tuple<CodeBlockResult, string[]>(isCorrect ? CodeBlockResult.SUCCESS : CodeBlockResult.FAILED, null);
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public async Task Create(CodeBlockTaskVm model)
        {
            try
            {
                CodeBlock codeBlock = new CodeBlock();
                codeBlock.Task = model.Task;
                foreach (TestCaseCodeBlock testCase in model.TestCases)
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
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                throw exception;
            }
            
        }
    }
}
