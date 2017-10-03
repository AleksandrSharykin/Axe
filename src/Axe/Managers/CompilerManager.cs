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
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.NodeServices;

namespace Axe.Managers
{
    /// <summary>
    /// Class implements operations which can be performed with <see cref="CodeBlock"/> entities
    /// </summary>
    public class CompilerManager : ManagerBase, ICompilerManager
    {
        private INodeServices nodeServices;

        public CompilerManager(AxeDbContext context, INodeServices nodeServices) : base(context)
        {
            this.nodeServices = nodeServices;
        }
        
        public async Task<List<CodeBlockVm>> GetCodeBlocks(int technologyId)
        {
            List<CodeBlockVm> list = await context.CodeBlock
                .Include(cb => cb.Technology)
                .Where(cb => (technologyId > 0) ? cb.Technology.Id == technologyId : cb.Technology.Id > 0)
                .Select(cb => new CodeBlockVm
                {
                    Id = cb.Id,
                    Task = cb.Task,
                    Technology = cb.Technology,
                })
                .ToListAsync();
          
            
            return list;
        }

        public async Task<CodeBlockSolveVm> GetCodeBlockById(int id)
        {
            CodeBlock codeBlock = await this.context.CodeBlock
                .Include(cb => cb.Technology)
                .FirstAsync(cb => cb.Id == id);
            CodeBlockSolveVm model = new CodeBlockSolveVm()
            {
                Id = codeBlock.Id,
                Task = codeBlock.Task,
                Technology = codeBlock.Technology,
                SourceCode = FormatCode(codeBlock.Technology.Template),
            };
            return model;
        }

        public async Task<CodeBlockCreateVm> GetByIdForEdit(int id)
        {
            CodeBlock codeBlock = await this.context.CodeBlock
                .Include(cb => cb.TestCases)
                .Include(cb => cb.Technology)
                .FirstAsync(cb => cb.Id == id);
            CodeBlockCreateVm model = new CodeBlockCreateVm()
            {
                Id = codeBlock.Id,
                Task = codeBlock.Task,
                OutputType = codeBlock.OutputType,
                TestCases = codeBlock.TestCases,
                SelectedTechnologyId = codeBlock.Technology.Id,
            };
            return model;
        }

        public async Task Create(CodeBlockCreateVm model)
        {
            CodeBlock codeBlock = new CodeBlock();
            codeBlock.Task = model.Task;
            codeBlock.TestCases = model.TestCases;
            codeBlock.OutputType = model.OutputType;
            codeBlock.VerificationCode = GenerateVerificationCodeCSharp(model);
            codeBlock.Technology = await context.Technology.FirstAsync(t => t.Id == model.SelectedTechnologyId);
            context.CodeBlock.Add(codeBlock);
            await context.SaveChangesAsync();
        }

        public async Task Update(CodeBlockCreateVm model)
        {
            var testCaseCodeBlocks = context.TestCaseCodeBlock.Where(ts => ts.codeBlock.Id == model.Id);
            context.TestCaseCodeBlock.RemoveRange(testCaseCodeBlocks);
            CodeBlock codeBlock = await this.context.CodeBlock
                .Include(cb => cb.TestCases)
                .Include(cb => cb.Technology)
                .FirstAsync(cb => cb.Id == model.Id);
            codeBlock.Id = model.Id;
            codeBlock.Task = model.Task;
            codeBlock.OutputType = model.OutputType;
            codeBlock.TestCases = model.TestCases;
            codeBlock.Technology = await context.Technology.FirstAsync(t => t.Id == model.SelectedTechnologyId);
            codeBlock.VerificationCode = GenerateVerificationCodeCSharp(model);
            await context.SaveChangesAsync();
        }

        public async Task DeleteById(int id)
        {
            CodeBlock codeBlock = await context.CodeBlock
                .Include(cb => cb.TestCases)
                .SingleAsync(cb => cb.Id == id);
            context.Remove(codeBlock);
            await context.SaveChangesAsync();
        }

        public async Task<Tuple<CodeBlockResult, string[]>> HandleCodeBlock(CodeBlockSolveVm model)
        {
            switch (model.Technology.Name)
            {
                case "C#":
                    return CompileAndExecuteCSharp(model);
                case "JavaScript":
                    return await InterpretJS(model);
                default:
                    throw new Exception("Unsupported technology is used");
            }
        }

        private Tuple<CodeBlockResult, string[]> CompileAndExecuteCSharp(CodeBlockSolveVm model)
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
                MetadataReference.CreateFromFile(typeof(System.Object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Collections.Hashtable).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).GetTypeInfo().Assembly.Location),
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

        private async Task<Tuple<CodeBlockResult, string[]>> InterpretJS(CodeBlockSolveVm model)
        {
            try
            {
                CodeBlock codeBlock = context.CodeBlock
                    .Include(cb => cb.TestCases)
                    .First(cb => cb.Id == model.Id);
                List<string> failedTestCases = new List<string>();

                string jsFilePath = Path.GetTempPath() + Path.GetRandomFileName().Replace(".", "") + ".js";
                for (int i = 0; i < codeBlock.TestCases.Count; i++)
                {
                    string code = @"module.exports = function (callback) {" + model.SourceCode + ";" +
                    "callback(null, main(" + codeBlock.TestCases[i].Input + ")); };";
                    using (var jsFile = File.Create(jsFilePath))
                    {
                        using (var stream = new StreamWriter(jsFile))
                        {
                            stream.Write(code);
                        }
                    }
                    object outputObj = await nodeServices.InvokeAsync<Object>(jsFilePath);   
                    nodeServices.Dispose();

                    string output = outputObj.ToString();
                    if (output != codeBlock.TestCases[i].Output)
                    {
                        failedTestCases.Add("Test case where input is " + codeBlock.TestCases[i].Input + " and output is " + codeBlock.TestCases[i].Output + " was failed");
                    }
                }
                if (File.Exists(jsFilePath))
                {
                    File.Delete(jsFilePath);
                }
                string[] info = new string[] { failedTestCases.Count + " of " + codeBlock.TestCases.Count + " testcases ended in failure" }.Union(failedTestCases).ToArray();
                return new Tuple<CodeBlockResult, string[]>((failedTestCases.Count == 0) ? CodeBlockResult.Success : CodeBlockResult.Failed, info);
            }
            catch (Exception exception)
            {
                return new Tuple<CodeBlockResult, string[]>(CodeBlockResult.Error, new string[] { exception.Message });
            }
        }
        
        public string FormatCode(string code)
        {
            return CSharpSyntaxTree.ParseText(code).GetRoot().NormalizeWhitespace().ToFullString();
        }

        private string GenerateVerificationCodeCSharp(CodeBlockCreateVm model)
        {
            string verificationCode =
                    @"resultsOfTestCases_#s# = new bool[#TEST_CASE_COUNT#];
                for (int i_#s# = 0; i_#s# < resultsOfTestCases_#s#.Length; i_#s#++)                    
                    resultsOfTestCases_#s#[i_#s#] = true;";
            for (int i = 0; i < model.TestCases.Count; i++)
            {
                verificationCode +=
                    @"#TYPE_FUNC# result_#s#_#INDEX# = Main(#INPUT#);";
                switch (model.OutputType)
                {
                    case SupportedType.Bool:
                    case SupportedType.Byte:
                    case SupportedType.Sbyte:
                    case SupportedType.Short:
                    case SupportedType.Ushort:
                    case SupportedType.Int:
                    case SupportedType.Uint:
                    case SupportedType.Long:
                    case SupportedType.Ulong:
                    case SupportedType.Double:
                    case SupportedType.Float:
                    case SupportedType.Decimal:
                    case SupportedType.Char:
                    case SupportedType.String:
                        {
                            verificationCode +=
                            @"if (result_#s#_#INDEX# != #OUTPUT#)
                                resultsOfTestCases_#s#[#INDEX#] = false;";
                            break;
                        }
                    case SupportedType.BoolArray:
                    case SupportedType.ByteArray:
                    case SupportedType.SbyteArray:
                    case SupportedType.ShortArray:
                    case SupportedType.UshortArray:
                    case SupportedType.IntArray:
                    case SupportedType.UintArray:
                    case SupportedType.LongArray:
                    case SupportedType.UlongArray:
                    case SupportedType.DoubleArray:
                    case SupportedType.FloatArray:
                    case SupportedType.DecimalArray:
                    case SupportedType.CharArray:
                    case SupportedType.StringArray:
                        {
                            verificationCode +=
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

                verificationCode = verificationCode
                    .Replace("#INDEX#", i.ToString())
                    .Replace("#OUTPUT#", model.TestCases[i].Output)
                    .Replace("#INPUT#", model.TestCases[i].Input);
            }

            DisplayAttribute attributeOfOutputType = model.OutputType.GetType()
                .GetMember(model.OutputType.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>();
            string outputTypeStr = attributeOfOutputType.Name;

            verificationCode = verificationCode
                .Replace("#TYPE_FUNC#", outputTypeStr);

            verificationCode = verificationCode
                .Replace("#TEST_CASE_COUNT#", model.TestCases.Count.ToString())
                .Replace("#s#", "AXE");
            return verificationCode;
        }
    }
}
