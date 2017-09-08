﻿using System;
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
            context.Database.EnsureCreated();

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
            };
            csharp.Experts = new List<ExpertTechnologyLink> { new ExpertTechnologyLink { User = superuser, Technology = csharp } };

            var javascript = new Technology
            {
                Name = "JavaScript",
                InformationText =
@"JavaScript is a high-level, dynamic, multi-paradigm, object-oriented, prototype-based, weakly-typed language traditionally used for client-side scripting in web browsers.",
            };
            javascript.Experts = new List<ExpertTechnologyLink> { new ExpertTechnologyLink { User = superuser, Technology = javascript } };

            context.AddRange(csharp, javascript);

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
                    Code = @"
                            using System;
                            namespace RoslynCompileSample
                            {
                                public class Writer
                                {
                                    public int Write(int[] array)
                                    {
                                        int maxValue = Int32.MinValue;
                                        for (int i = 0; i < array.Length; i++)
                                        {
                                            if (array[i] > maxValue) maxValue = array[i];
                                        }
                                        return maxValue;
                                    }
                                }
                            }",
                    Output = "59334",
                    Task = "Write a program to find max element of array"
                },
                new CodeBlock
                {
                    Code = @"
                            Sample code",
                    Output = "1",
                    Task = "Write a program..."
                }
            };

            context.CodeBlock.AddRange(codeBlocks);

            #endregion

            await context.SaveChangesAsync();
        }
    }
}
