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
                model.SourceCode = FormatCode(model.SourceCode);
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
                MethodDeclarationSyntax methodCheck = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "CHECK")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBody(SyntaxFactory.Block(syntaxOfVerificationCode));

                string arrayName = "resultsOfTestCases_#s#".Replace("#s#", "AXE");
                VariableDeclarationSyntax arrayOfResults = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("bool []"))
                    .AddVariables(SyntaxFactory.VariableDeclarator(arrayName));
                FieldDeclarationSyntax arrayField = SyntaxFactory.FieldDeclaration(arrayOfResults);

                string parameterName = "i_#s#".Replace("#s#", "AXE");
                ParameterSyntax parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterName))
                    .WithType(SyntaxFactory.ParseTypeName("int"));
                string codeOfFunction = @"return resultsOfTestCases_#s#[i_#s#];".Replace("#s#", "AXE"); ;
                StatementSyntax syntaxOfFunction = SyntaxFactory.ParseStatement(codeOfFunction);
                MethodDeclarationSyntax methodGetResult = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("bool"), "GET_RESULT_OF_TEST_CASE")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddParameterListParameters(parameter)
                    .WithBody(SyntaxFactory.Block(syntaxOfFunction));

                ClassDeclarationSyntax axeTaskClass = syntaxTree.GetRoot().DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .First(c => c.Identifier.ValueText == "AxeTask");

                ClassDeclarationSyntax newAxeTaskClass = axeTaskClass.AddMembers(arrayField, methodCheck, methodGetResult);
     
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
                        return new Tuple<CodeBlockResult, string[]>(CodeBlockResult.Error, errors);
                    }
                    else
                    {
                        stream.Seek(0, SeekOrigin.Begin);

                        AssemblyLoadContext context = AssemblyLoadContext.Default;
                        Assembly assembly = context.LoadFromStream(stream);

                        MethodInfo check = assembly.GetType("Axe.AxeTask").GetMethod("CHECK");
                        Object axeClass = assembly.CreateInstance("Axe.AxeTask");
                        check.Invoke(axeClass, null);

                        MethodInfo getResult = assembly.GetType("Axe.AxeTask").GetMethod("GET_RESULT_OF_TEST_CASE");
                        List<string> failedTestCases = new List<string>();
                        for (int i = 0; i < codeBlock.TestCases.Count; i++)
                        {
                            Object output = getResult.Invoke(axeClass, new object[] { i });
                            if (!(bool)output)
                            {
                                failedTestCases.Add("Test case where input is " + codeBlock.TestCases[i].Input + " and output is " + codeBlock.TestCases[i].Output + " was failed");
                            }
                        }
                        string[] info = new string[] { failedTestCases.Count + " of " + codeBlock.TestCases.Count + " testcases ended in failure" }.Union(failedTestCases).ToArray();
                        return new Tuple<CodeBlockResult, string[]>((failedTestCases.Count == 0) ? CodeBlockResult.Success : CodeBlockResult.Failed, info);
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
                    @"resultsOfTestCases_#s# = new bool[#TEST_CASE_COUNT#];
                for (int i_#s# = 0; i_#s# < resultsOfTestCases_#s#.Length; i_#s#++)                    
                    resultsOfTestCases_#s#[i_#s#] = true;";
                for (int i = 0; i < codeBlock.TestCases.Count; i++)
                {
                    codeBlock.VerificationCode +=
                        @"#TYPE_FUNC# result_#s#_#INDEX# = Main(#INPUT#);";
                    switch (codeBlock.OutputType)
                    {
                        case SupportedType.Int:
                        case SupportedType.Double:
                        case SupportedType.String:
                            {
                                codeBlock.VerificationCode +=
                                @"if (result_#s#_#INDEX# != #OUTPUT#)
                                resultsOfTestCases_#s#[#INDEX#] = false;";
                                break;
                            }
                        case SupportedType.IntArray:
                        case SupportedType.DoubleArray:
                        case SupportedType.StringArray:
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
                    case SupportedType.Int:
                        {
                            codeBlock.VerificationCode = codeBlock.VerificationCode
                                .Replace("#TYPE_FUNC#", "int");
                            break;
                        }
                    case SupportedType.Double:
                        {
                            codeBlock.VerificationCode = codeBlock.VerificationCode
                                .Replace("#TYPE_FUNC#", "double");
                            break;
                        }
                    case SupportedType.String:
                        {
                            codeBlock.VerificationCode = codeBlock.VerificationCode
                                .Replace("#TYPE_FUNC#", "string");
                            break;
                        }
                    case SupportedType.IntArray:
                        {
                            codeBlock.VerificationCode = codeBlock.VerificationCode
                                .Replace("#TYPE_FUNC#", "int[]");
                            break;
                        }
                    case SupportedType.DoubleArray:
                        {
                            codeBlock.VerificationCode = codeBlock.VerificationCode
                                .Replace("#TYPE_FUNC#", "double[]");
                            break;
                        }
                    case SupportedType.StringArray:
                        {
                            codeBlock.VerificationCode = codeBlock.VerificationCode
                                .Replace("#TYPE_FUNC#", "string[]");
                            break;
                        }
                    default:
                        break;
                }

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

        public string FormatCode(string code)
        {
            return CSharpSyntaxTree.ParseText(code).GetRoot().NormalizeWhitespace().ToFullString();
        }
    }
}
