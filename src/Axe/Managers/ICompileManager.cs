using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Axe.Models;
using Axe.ViewModels.CompilerVm;

namespace Axe.Managers
{
    /// <summary>
    /// Interface declares operations which can be performed with <see cref="CodeBlock"/>  ennities
    /// </summary>
    public interface ICompileManager
    {
        /// <summary>
        /// Returns a list of code block available for current user
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<List<CodeBlockVm>> GetCodeBlocks();

        Task<CodeBlockVm> GetById(int id);

        Tuple<CodeBlockResult, string[]> Solve(CodeBlockVm model);

        Task Create(CodeBlockTaskVm model);
    }
}
