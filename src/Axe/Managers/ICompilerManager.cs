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
        Task<List<T>> GetCodeBlocks<T>(int technologyId) where T : class;

        /// <summary>
        /// Returns a code block with id
        /// </summary>
        /// <param name="id">Identifier of code block</param>
        /// <returns>CodeBlockVm</returns>
        Task<T> GetCodeBlockById<T>(int id) where T : class;

        /// <summary>
        /// Updates a code block
        /// </summary>
        /// <param name="model">CodeBlockTaskVm</param>
        /// <returns></returns>
        Task Update(CodeBlockCreateVm model);

        /// <summary>
        /// Deletes a code block with id
        /// </summary>
        /// <param name="id">Identifier of code block</param>
        /// <returns></returns>
        Task DeleteById(int id);

        /// <summary>
        /// Creates and adds new code block task
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task Create(CodeBlockCreateVm model);

        /// <summary>
        /// Solves and returns code block result 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<CodeBlockResultVm> HandleCodeBlock(CodeBlockCheckVm model);

        /// <summary>
        /// Allows to get all attempts for user
        /// </summary>
        /// <typeparam name="T">View model class which represents attempt</typeparam>
        /// <param name="userId">User identifier</param>
        /// <returns>List attempts</returns>
        Task<List<T>> GetAttempts<T>(string userId) where T : class;

        /// <summary>
        /// Allows to get specific attempt for user
        /// </summary>
        /// <typeparam name="T">View model class which represents attempt</typeparam>
        /// <param name="userId">User identifier</param>
        /// <param name="taskId">Task identifier</param>
        /// <returns></returns>
        Task<T> GetAttempt<T>(string userId, int taskId) where T : class;

        /// <summary>
        /// Allows to save attempt
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="codeBlockId"></param>
        /// <param name="sourceCode"></param>
        /// <returns></returns>
        Task SaveAttempt(string userId, int codeBlockId, string sourceCode);
    }
}
