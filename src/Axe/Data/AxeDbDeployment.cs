using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Axe.Models
{
    public static class AxeDbDeployment
    {
        public static async Task Deploy(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, AxeDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            if (context.Roles.Any())
            {
                // db has been configurated already
                return;
            }

            await roleManager.CreateAsync(new IdentityRole(UserRole.Superuser));
            await roleManager.CreateAsync(new IdentityRole(UserRole.Member));

            var superuser = new ApplicationUser
            {
                UserName = "superuser@supermail.com",
                Email = "superuser@supermail.com",
                JobPosition = "admin",
            };

            await userManager.CreateAsync(superuser, "T0pSecret");
            await userManager.AddToRolesAsync(superuser, new string[] { UserRole.Superuser, UserRole.Member });

            #region Technologies
            if (context.Technology.Any())
            {
                // data has been added already
                return;
            }

            var csharp = new Technology
            {
                Name = "C#",
                InformationText =
@"C# is a programming language that is designed for building a variety of applications that run on the .NET Framework.
C# is simple, powerful, type-safe, and object-oriented",
                Template = "using System;\nnamespace Axe\n{\npublic class AxeTask\n{\npublic int Main()\n{\n}\n}\n}",
            };
            csharp.Experts = new List<ExpertTechnologyLink> { new ExpertTechnologyLink { User = superuser, Technology = csharp } };

            var javascript = new Technology
            {
                Name = "JavaScript",
                InformationText =
@"JavaScript is a high-level, dynamic, multi-paradigm, object-oriented, prototype-based, weakly-typed language traditionally used for client-side scripting in web browsers.",
                Template = "function main() {\n\n}",
            };
            javascript.Experts = new List<ExpertTechnologyLink> { new ExpertTechnologyLink { User = superuser, Technology = javascript } };

            var python = new Technology
            {
                Name = "Python",
                InformationText = "Python is ...",
                Template = "def main():\n\t#body",
            };
            python.Experts = new List<ExpertTechnologyLink> { new ExpertTechnologyLink { User = superuser, Technology = python } };
            context.AddRange(csharp, javascript, python);

            #endregion

            var task = new ExamTask
            {
                Title = "C# demo",
                Objective = "Axe system demonstration",
                Author = superuser,
                IsDemonstration = true,
                Technology = csharp,
            };

            #region C# questions

            var questionPriority = new TaskQuestion
            {
                Author = superuser,
                Technology = csharp,
                Type = TaskQuestionType.PrioritySelection,
                Text = "Interface should declare Print method which accept values of any type and writes them in Console." + Environment.NewLine
                + "Method doesn't return result." + Environment.NewLine
                + "Construct correct method declaration using the following blocks:",
            };
            questionPriority.Answers = new List<TaskAnswer>
            {
                new TaskAnswer
                {
                    Question = questionPriority,
                    Text = "public",
                    Value = null,
                    Score = 0,
                },
                new TaskAnswer
                {
                    Question = questionPriority,
                    Text = "abstract",
                    Value = null,
                    Score = 0,
                },
                new TaskAnswer
                {
                    Question = questionPriority,
                    Text = "void",
                    Value = "1",
                    Score = 1,
                },
                new TaskAnswer
                {
                    Question = questionPriority,
                    Text = "object",
                    Value = null,
                    Score = 0,
                },
                new TaskAnswer
                {
                    Question = questionPriority,
                    Text = "Print",
                    Value = "2",
                    Score = 1,
                },
                new TaskAnswer
                {
                    Question = questionPriority,
                    Text = "(object inputValue)",
                    Value = "3",
                    Score = 1,
                },
                new TaskAnswer
                {
                    Question = questionPriority,
                    Text = "{ }",
                    Value = null,
                    Score = 0,
                },
            };


            var questionMultiChoice = new TaskQuestion
            {
                Author = superuser,
                Technology = csharp,
                Type = TaskQuestionType.MultiChoice,
                Text = "Possible access modifiers for classes are ...",
            };
            questionMultiChoice.Answers = new List<TaskAnswer>
            {
                new TaskAnswer
                {
                    Question = questionMultiChoice,
                    Text = "public",
                    Value = Boolean.TrueString,
                    Score = 1,
                },
                new TaskAnswer
                {
                    Question = questionMultiChoice,
                    Text = "protected",
                    Value = Boolean.FalseString,
                    Score = 0,
                },
                new TaskAnswer
                {
                    Question = questionMultiChoice,
                    Text = "private",
                    Value = Boolean.TrueString,
                    Score = 1,
                },
                new TaskAnswer
                {
                    Question = questionMultiChoice,
                    Text = "internal",
                    Value = Boolean.TrueString,
                    Score = 1,
                },
            };


            var questionSingleChoice = new TaskQuestion
            {
                Author = superuser,
                Technology = csharp,
                Type = TaskQuestionType.SingleChoice,
                Text = "System name of float type is ...",
            };

            questionSingleChoice.Answers = new List<TaskAnswer>
            {
                new TaskAnswer
                {
                    Question = questionSingleChoice,
                    Text = "float",
                    Value = Boolean.FalseString,
                    Score = 0,
                },
                new TaskAnswer
                {
                    Question = questionSingleChoice,
                    Text = "System.Float",
                    Value = Boolean.FalseString,
                    Score = 0,
                },
                new TaskAnswer
                {
                    Question = questionSingleChoice,
                    Text = "System.Single",
                    Value = Boolean.TrueString,
                    Score = 1,
                },
                new TaskAnswer
                {
                    Question = questionSingleChoice,
                    Text = "System.Short",
                    Value = Boolean.FalseString,
                    Score = 0,
                },
            };


            var questionMultiInput = new TaskQuestion
            {
                Author = superuser,
                Technology = csharp,
                Type = TaskQuestionType.MultiLine,
                Text = "Write results of the following operations:" + Environment.NewLine + "10/4" + Environment.NewLine + "\"1\"+\"1\"",
            };
            var answerMultiInput = new TaskAnswer
            {
                Question = questionMultiInput,
                Text = "2" + Environment.NewLine + "\"11\"",
                Score = 2,
            };
            answerMultiInput.Value = answerMultiInput.Text;
            questionMultiInput.Answers = new List<TaskAnswer> { answerMultiInput };


            var questionSingleInput = new TaskQuestion
            {
                Author = superuser,
                Technology = csharp,
                Type = TaskQuestionType.SingleLine,
                Text = "Max value of Byte type is",
            };
            var answerSingleInput = new TaskAnswer
            {
                Question = questionSingleInput,
                Text = "255",
                Score = 1,
            };
            answerSingleInput.Value = answerSingleInput.Text;

            questionSingleInput.Answers = new List<TaskAnswer> { answerSingleInput };

            #endregion

            task.Questions = new List<TaskQuestionLink>
            {
                new TaskQuestionLink { Task = task, Question = questionMultiChoice, },
                new TaskQuestionLink { Task = task, Question = questionSingleChoice, },
                new TaskQuestionLink { Task = task, Question = questionMultiInput, },
                new TaskQuestionLink { Task = task, Question = questionSingleInput, },
                new TaskQuestionLink { Task = task, Question = questionPriority, },
            };

            context.Add(task);

            #region Code Blocks

            var codeBlocks = new CodeBlock[]
            {
                new CodeBlock {
                    Task = "Write a program to find max element of array. Input data is array (eg. { 6, 3, 1, 59334, 232, 3, -1 }). Program must return 59334. " +
                    "Main method signature is (int[]). " +
                    "You mustn't change names of namespace, class and main method.",
                    TestCases = new List<TestCaseCodeBlock> {
                        new TestCaseCodeBlock { Input = "new int[] { 6, 3, 1, 59334, 232, 3, -1 }", Output = "59334" },
                        new TestCaseCodeBlock { Input = "new int[] { 5, 745, 2, 7434 }", Output = "7434" } },
                    VerificationCode = @"resultsOfTestCases_AXE = new bool[2];
                    for (int i_AXE = 0; i_AXE < resultsOfTestCases_AXE.Length; i_AXE++)                    
                        resultsOfTestCases_AXE[i_AXE] = true;
                    
                    int result_AXE_0 = Main(new int[] { 6, 3, 1, 59334, 232, 3, -1 });
                    if (result_AXE_0 != 59334)
                        resultsOfTestCases_AXE[0] = false;
                    
                    int result_AXE_1 = Main(new int[] { 5, 745, 2, 7434 });
                    if (result_AXE_1 != 7434 )
                        resultsOfTestCases_AXE[1] = false;",
                    OutputType = SupportedType.Int,
                    Technology = csharp,
                },
                new CodeBlock {
                    Task = "Write a program to sum of two number. Input data is two number (eg. 15.4, 6.6). Program must return 22. " +
                    "The entrance function is \"main\". " +
                    "You mustn't change name of main function.",
                    TestCases = new List<TestCaseCodeBlock> {
                        new TestCaseCodeBlock { Input = "20, 5", Output = "25" },
                        new TestCaseCodeBlock { Input = "15.4, 6.6", Output = "22" } },
                    VerificationCode = @"",
                    OutputType = SupportedType.Int,
                    Technology = javascript,
                },
                new CodeBlock
                {
                    Task = "Python",
                    TestCases = new List<TestCaseCodeBlock> {
                        new TestCaseCodeBlock { Input = "20.0 4.0", Output = "80.0" },
                        new TestCaseCodeBlock { Input = "15.4 6.6", Output = "101.64" } },
                    VerificationCode = @"",
                    OutputType = SupportedType.Int,
                    Technology = python,
                }
            };
            
            context.CodeBlock.AddRange(codeBlocks);

            #endregion

            await context.SaveChangesAsync();
        }
    }
}
