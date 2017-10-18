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
using System.Diagnostics;
using AutoMapper;
using System.Text;

namespace Axe.Managers
{
    /// <summary>
    /// Class implements operations which can be performed with <see cref="CodeBlock"/> entities
    /// </summary>
    public class CompilerManager : ManagerBase, ICompilerManager
    {
        private readonly ITechnologyManager technologyManager;
        private readonly INodeServices nodeServices;
        private readonly IMapper mapper;

        public CompilerManager(AxeDbContext context, ITechnologyManager technologyManager, INodeServices nodeServices, IMapper mapper) : base(context)
        {
            this.technologyManager = technologyManager;
            this.nodeServices = nodeServices;
            this.mapper = mapper;
        }

        public async Task<List<T>> GetCodeBlocks<T>(int technologyId) where T:class
        {
            var codeBlocks = await context.CodeBlock
                .Where(cb => (technologyId > 0) ? cb.Technology.Id == technologyId : cb.Technology.Id > 0)
                .Include(cb => cb.Technology)
                .ToListAsync();

            var model = mapper.Map<List<T>>(codeBlocks);
            return model;
        }

        public async Task<T> GetCodeBlockById<T>(int id) where T: class
        {
            var codeBlock = await this.context.CodeBlock
                .Include(cb => cb.TestCases)
                .Include(cb => cb.Technology)
                .FirstAsync(cb => cb.Id == id);

            var model = mapper.Map<T>(codeBlock);
            return model;
        }

        public async Task Create(CodeBlockCreateVm model)
        {
            var codeBlock = new CodeBlock();
            codeBlock.Task = model.Task;
            codeBlock.TestCases = model.TestCases;
            codeBlock.OutputType = model.OutputType;
            codeBlock.TechnologyId = model.SelectedTechnologyId;
            var technology = await technologyManager.GetTechnologyById(model.SelectedTechnologyId);
            codeBlock.VerificationCode = technology.Name == "C#" ? GenerateVerificationCodeCSharp(model) : "";
            context.CodeBlock.Add(codeBlock);
            await context.SaveChangesAsync();
        }

        public async Task Update(CodeBlockCreateVm model)
        {
            var testCaseCodeBlocks = context.TestCaseCodeBlock.Where(ts => ts.codeBlock.Id == model.Id);
            context.TestCaseCodeBlock.RemoveRange(testCaseCodeBlocks);
            var codeBlock = await this.context.CodeBlock
                .Include(cb => cb.TestCases)
                .Include(cb => cb.Technology)
                .FirstAsync(cb => cb.Id == model.Id);
            codeBlock.Task = model.Task;
            codeBlock.OutputType = model.OutputType;
            codeBlock.TestCases = model.TestCases;
            codeBlock.TechnologyId = model.SelectedTechnologyId;
            var technology = await technologyManager.GetTechnologyById(model.SelectedTechnologyId);
            codeBlock.VerificationCode = technology.Name == "C#" ? GenerateVerificationCodeCSharp(model) : "";
            await context.SaveChangesAsync();
        }

        public async Task DeleteById(int id)
        {
            var codeBlock = await context.CodeBlock
                .Include(cb => cb.TestCases)
                .FirstAsync(cb => cb.Id == id);
            context.Remove(codeBlock);
            await context.SaveChangesAsync();
        }

        public async Task<CodeBlockResultVm> HandleCodeBlock(CodeBlockCheckVm model)
        {
            switch (model.TechnologyName)
            {
                case "C#":
                    return CompileAndExecuteCSharp(model);
                case "JavaScript":
                    return await InterpretJS(model);
                case "Python":
                    return await InterpretPython(model);
                default:
                    throw new Exception("Unsupported technology is used");
            }
        }

        private CodeBlockResultVm CompileAndExecuteCSharp(CodeBlockCheckVm model)
        {
            CodeBlock codeBlock = context.CodeBlock
                .Include(cb => cb.TestCases)
                .First(cb => cb.Id == model.Id);

            string assemblyName = "AxeCompilerAssembly" + Path.GetRandomFileName();
            // Random file name is necessary because .NETCore doesn't allow to unload assembly
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(model.SourceCode);

            // Declaration of the verification function
            StatementSyntax syntaxOfVerificationCode = SyntaxFactory.ParseStatement(codeBlock.VerificationCode);
            MethodDeclarationSyntax methodCheck = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "CHECK")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBody(SyntaxFactory.Block(syntaxOfVerificationCode));

            // Declaration of array with test case results
            string arrayName = "resultsOfTestCases_#s#".Replace("#s#", "AXE");
            VariableDeclarationSyntax arrayOfResults = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("bool []"))
                .AddVariables(SyntaxFactory.VariableDeclarator(arrayName));
            FieldDeclarationSyntax arrayField = SyntaxFactory.FieldDeclaration(arrayOfResults);

            // Declaration of the function which requires number of test case and returns result of this test case
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
                    return new CodeBlockResultVm { TypeResult = CodeBlockResult.Error, Content = errors };
                }
                else
                {
                    stream.Seek(0, SeekOrigin.Begin);

                    AssemblyLoadContext context = AssemblyLoadContext.Default;
                    Assembly assembly = context.LoadFromStream(stream);

                    // Invoke the method CHECK which performs test cases
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
                            failedTestCases.Add("Test case where input is " + (codeBlock.TestCases[i].Input ?? " ") + " and output is " + codeBlock.TestCases[i].Output + " was failed");
                        }
                    }
                    if (failedTestCases.Count > 0)
                    {
                        failedTestCases.Add(failedTestCases.Count + " of " + codeBlock.TestCases.Count + " testcases ended in failure");
                    }

                    return new CodeBlockResultVm
                    {
                        TypeResult = (failedTestCases.Count == 0) ? CodeBlockResult.Success : CodeBlockResult.Failed,
                        Content = failedTestCases.ToArray(),
                    };
                }
            }
        }
        
        private async Task<CodeBlockResultVm> InterpretJS(CodeBlockCheckVm model)
        {
            try
            {
                var codeBlock = await GetCodeBlockById<CodeBlock>(model.Id);
                List<string> failedTestCases = new List<string>();

                string jsFilePath = Path.GetTempPath() + Path.GetRandomFileName().Replace(".", "") + ".js";
                foreach (var testCase in codeBlock.TestCases)
                {
                    // Generate the code for execution
                    var codeSb = new StringBuilder("module.exports = function (callback) {");
                    codeSb.AppendJoin("", model.SourceCode, ";", "callback(null, equal_AXE(", testCase.Output, ", " +
                        "main(", (testCase.Input ?? " "), "))",
                        "); };");
                    
                    codeSb.Append(GetVerificationCodeJavaScript());
                    // Write the code into temporary file
                    using (var jsFile = File.Create(jsFilePath))
                    {
                        using (var stream = new StreamWriter(jsFile))
                        {
                            stream.Write(codeSb.ToString());
                        }
                    }
                    // Get result from node services
                    bool output = await nodeServices.InvokeAsync<bool>(jsFilePath);
                    nodeServices.Dispose();

                    if (!output)
                    {
                        failedTestCases.Add("Test case where input is " + (testCase.Input ?? " ") + " and output is " + testCase.Output + " was failed");
                    }
                }
                if (File.Exists(jsFilePath))
                {
                    File.Delete(jsFilePath);
                }
                if (failedTestCases.Count > 0)
                {
                    failedTestCases.Add(failedTestCases.Count + " of " + codeBlock.TestCases.Count + " testcases ended in failure");
                }
                return new CodeBlockResultVm
                {
                    TypeResult = (failedTestCases.Count == 0) ? CodeBlockResult.Success : CodeBlockResult.Failed,
                    Content = failedTestCases.ToArray(),
                };
            }
            catch (Exception exception)
            {
                return new CodeBlockResultVm
                {
                    TypeResult = CodeBlockResult.Error,
                    Content = new string[] { exception.Message },
                };
            }
        }
        
        private async Task<CodeBlockResultVm> InterpretPython(CodeBlockCheckVm model)
        {
            try
            {
                var codeBlock = await GetCodeBlockById<CodeBlock>(model.Id);
                List<string> failedTestCases = new List<string>();
                string pyFilePath = Path.GetTempPath() + Path.GetRandomFileName().Replace(".", "") + ".py";

                ProcessStartInfo startInfo = new ProcessStartInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\Local\\Programs\\Python\\Python36-32\\python.exe");
                startInfo.Arguments = pyFilePath;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;

                foreach (var testCase in codeBlock.TestCases)
                {
                    // Generate the code for execution
                    var codeSb = new StringBuilder(model.SourceCode);
                    if (testCase.Input != null)
                    {
                        string[] inputArguments = testCase.Input.Split(' ');
                        codeSb.Append("\nprint(main(");
                        for (int j = 0; j < inputArguments.Length; j++)
                        {
                            codeSb.Append(inputArguments[j]);
                            if (j < inputArguments.Length - 1)
                            {
                                codeSb.Append(", ");
                            }
                        }
                        codeSb.Append("));");
                    }
                    else
                    {
                        codeSb.Append("\nprint(main());");
                    }
                    // Write the code into temporary file
                    using (var pyFile = File.Create(pyFilePath))
                    {
                        using (var stream = new StreamWriter(pyFile))
                        {
                            stream.Write(codeSb.ToString());
                        }
                    }
                    // Create process for Python and execute script
                    Process process = new Process();
                    process.StartInfo = startInfo;
                    process.Start();
                    // Read data from terminal
                    string error;
                    using (var stream = process.StandardError)
                    {
                        error = await stream.ReadToEndAsync();
                    }
                    if (error.Length > 0)
                    {
                        failedTestCases.Add(error);
                        break;
                    }

                    string output;
                    using (var stream = process.StandardOutput)
                    {
                        output = await stream.ReadToEndAsync();
                    }
                    output = output.Replace("\r\n", ""); // Console returnes this symbol in the end of the string
                    if (output != testCase.Output)
                    {
                        failedTestCases.Add("Test case where input is " + (testCase.Input ?? " ") + " and output is " + testCase.Output + " was failed");
                    }

                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }
                if (File.Exists(pyFilePath))
                {
                    File.Delete(pyFilePath);
                }
                if (failedTestCases.Count > 0)
                {
                    failedTestCases.Add(failedTestCases.Count + " of " + codeBlock.TestCases.Count + " testcases ended in failure");
                }
                return new CodeBlockResultVm
                {
                    TypeResult = (failedTestCases.Count == 0) ? CodeBlockResult.Success : CodeBlockResult.Failed,
                    Content = failedTestCases.ToArray(),
                };
            }
            catch (Exception exception)
            {
                return new CodeBlockResultVm
                {
                    TypeResult = CodeBlockResult.Error,
                    Content = new string[] { exception.Message },
                };
            }
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
                    .Replace("#INPUT#", model.TestCases[i].Input ?? "");
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

        private string GetVerificationCodeJavaScript() => @"
            function equal_AXE(x, y) {
                if (typeof x == typeof y) {
                    switch (typeof x) {
                        case 'object':
                            {
                                // Handle null
                                if (x === null || y === null) {
                                    if (x === null && y === null) {
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                return objectsIsEqual_AXE(x, y);
                            }
                        case 'number':
                        case 'string':
                        case 'boolean':
                        case 'undefined':
                            {
                                return x === y;
                            }
                        case 'function':
                            {
                                // Skip function type
                                return true;
                            }
                        default:
                            {
                                return false;
                            }
                    }
                }
                else {
                    return false;
                }
            };

            function objectsIsEqual_AXE(a, b) {
                var aProps = Object.getOwnPropertyNames(a);
                var bProps = Object.getOwnPropertyNames(b);
                if (aProps.length != bProps.length) {
                    return false;
                }
                for (var i = 0; i < aProps.length; i++) {
                    var propName = aProps[i];

                    if (!equal_AXE(a[propName], b[propName]))
                    {
                        return false;
                    }
                }
                return true;
            };";

        public async Task<List<T>> GetAttempts<T>(string userId) where T: class
        {
            var attempts = await context.AttemptCodeBlock
                .Where(acb => acb.User.Id == userId)
                .Include(acb => acb.CodeBlock)
                .ThenInclude(cb => cb.Technology)
                .ToListAsync();

            var model = mapper.Map<List<T>>(attempts);
            return model;
        }

        public async Task<T> GetAttempt<T>(string userId, int taskId) where T: class
        {
            var attempt = await context.AttemptCodeBlock
                .Where(acb => acb.User.Id == userId)
                .Include(acb => acb.CodeBlock)
                .FirstOrDefaultAsync(acb => acb.CodeBlock.Id == taskId);
            if (attempt == null) return null;

            var model = mapper.Map<T>(attempt);
            return model;
        }

        public async Task SaveAttempt(string userId, int codeBlockId, string sourceCode)
        {
            var attempt = await context.AttemptCodeBlock.FirstOrDefaultAsync(acb => acb.User.Id == userId && acb.CodeBlock.Id == codeBlockId);
            if (attempt == null)
            {
                attempt = new AttemptCodeBlock
                {
                    SourceCode = sourceCode,
                    UserId = userId,
                    CodeBlockId = codeBlockId,
                    DateLastChanges = DateTime.Now,
                };
                await context.AttemptCodeBlock.AddAsync(attempt);
            }
            else
            {
                attempt.DateLastChanges = DateTime.Now;
                attempt.SourceCode = sourceCode;
            }
            await context.SaveChangesAsync();
        }
    }
}
