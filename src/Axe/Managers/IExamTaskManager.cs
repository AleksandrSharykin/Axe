using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Axe.Models;
using Axe.Models.ExamTasksVm;

namespace Axe.Managers
{
    /// <summary>
    /// Interface declares operations governed by <see cref="Axe.Controllers.TasksController"/>
    /// </summary>
    public interface IExamTaskManager
    {
        /// <summary>
        /// Gets <see cref="ExamTask"/> details for display
        /// </summary>
        Task<Response<ExamTask>> DetailsGet(Request<int?> request);

        /// <summary>
        /// Gets <see cref="ExamTask"/> object template for creation or edit
        /// </summary>
        Task<Response<TaskInputVm>> InputGet(Request<ExamTask> request);

        /// <summary>
        /// Applies <see cref="ExamTask"/> edit results
        /// </summary>
        Task<Response<TaskInputVm>> InputPost(Request<TaskInputVm> request);

        /// <summary>
        /// Gets <see cref="ExamTask"/> object for preview before deletion
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Response<ExamTask>> DeleteGet(Request<int> request);

        /// <summary>
        /// Deletes <see cref="ExamTask"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Response<ExamTask>> DeletePost(Request<int> request);
    }
}
