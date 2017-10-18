using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Axe.Managers;
using Axe.ViewModels.CompilerVm;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Axe.Models;
using Newtonsoft.Json;

namespace Axe.Controllers
{
    [Produces("application/json")]
    [Route("api/compiler/[action]")]
    public class CompilerApiController : Controller
    {
        private readonly ICompilerManager compilerManager;
        private readonly UserManager<ApplicationUser> userManager;

        public CompilerApiController(ICompilerManager compilerManager, UserManager<ApplicationUser> userManager)
        {
            this.compilerManager = compilerManager;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<CodeBlockVm> GetTaskById(int id)
        {
            try
            {
                var codeBlock = await compilerManager.GetCodeBlockById<CodeBlockVm>(id);
                return codeBlock;
            }
            catch
            {
                return null;
            }
        }

        [Authorize]
        [HttpPost]
        public async Task SaveAttempt(int codeBlockId, string sourceCode)
        {
            string userId = userManager.GetUserId(this.HttpContext.User);
            await compilerManager.SaveAttempt(userId, codeBlockId, sourceCode);
        }

        [Authorize]
        [HttpPost]
        public async Task<string> Check(CodeBlockCheckVm model)
        {
            try
            {
                var result = await compilerManager.HandleCodeBlock(model);
                return JsonConvert.SerializeObject(result);
            }
            catch (Exception exception)
            {
                return JsonConvert.SerializeObject(new CodeBlockResultVm
                {
                    TypeResult = CodeBlockResult.Error,
                    Content = new string[] { exception.Message },
                });
            }
        }
    }
}
