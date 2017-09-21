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
    public interface ICompilerManager
    {
        /// <summary>
        /// Returns a list of code block available for current user
        /// </summary>
        /// <returns>List of CodeBlockVm</returns>
        Task<List<CodeBlockVm>> GetCodeBlocks();

        /// <summary>
        /// Returns a code block with id
        /// </summary>
        /// <param name="id">Identifier of code block</param>
        /// <returns>CodeBlockVm</returns>
        Task<CodeBlockVm> GetById(int id);

        /// <summary>
        /// Returns a code block with id
        /// </summary>
        /// <param name="id">Identifier of code block</param>
        /// <returns>CodeBlockTaskVm</returns>
        Task<CodeBlockTaskVm> GetByIdForEdit(int id);

        /// <summary>
        /// Updates a code block
        /// </summary>
        /// <param name="model">CodeBlockTaskVm</param>
        /// <returns></returns>
        Task Update(CodeBlockTaskVm model);

        /// <summary>
        /// Deletes a code block with id
        /// </summary>
        /// <param name="id">Identifier of code block</param>
        /// <returns></returns>
        Task DeleteById(int id);

        /// <summary>
        /// Solves and returns code block result 
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Tuple, where Item1 - type of result and Item2 - array of string which contains error if they were</returns>
        Tuple<CodeBlockResult, string[]> Solve(CodeBlockVm model);

        /// <summary>
        /// Creates and adds new code block task
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task Create(CodeBlockTaskVm model);

        /// <summary>
        /// Returns formatted code
        /// </summary>
        /// <param name="code"></param>
        /// <returns>Full string of code with normalized whitespaces</returns>
        string FormatCode(string code);
    }
}
