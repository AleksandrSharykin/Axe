using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Axe.Models;
using Axe.Models.QuestionsVm;

namespace Axe.Managers
{
    /// <summary>
    /// Interface declares operations governed by <see cref="Axe.Controllers.QuestionsController"/>
    /// </summary>
    public interface IQuestionManager
    {
        /// <summary>
        /// Gets <see cref="TaskQuestion"/> object template for creation or edit
        /// </summary>
        Task<Response<QuestionInputVm>> InputGet(Request<TaskQuestion> request);

        /// <summary>
        /// Applies <see cref="TaskQuestion"/> edit results
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Response<QuestionInputVm>> InputPost(Request<QuestionInputVm> request);

        /// <summary>
        /// Add or removes answer options from a question with choice
        /// </summary>
        /// <param name="request"></param>
        /// <param name="adding"></param>
        /// <returns></returns>
        Response<QuestionInputVm> ChangeAnswers(Request<QuestionInputVm> request, bool adding);

        /// <summary>
        /// Changes question type
        /// </summary>
        /// <param name="request"></param>
        /// <param name="etp"></param>
        /// <returns></returns>
        Response<QuestionInputVm> ChangeQuestionType(Request<QuestionInputVm> request, TaskQuestionType etp);

        /// <summary>
        /// Gets <see cref="TaskQuestion"/> object for preview
        /// </summary>
        Task<Response<TaskQuestion>> DetailsGet(Request<int> request);

        /// <summary>
        /// Gets <see cref="TaskQuestion"/> object for preview before deletion
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Response<TaskQuestion>> DeleteGet(Request<int> request);

        /// <summary>
        /// Deletes <see cref="TaskQuestion"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Response<TaskQuestion>> DeletePost(Request<int> request);
    }
}
