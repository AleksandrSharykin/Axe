using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Axe.Models;

namespace Axe.Managers
{
    /// <summary>
    /// Interface declares operations which can be performed with <see cref="ExamAttempt"/>  ennities
    /// </summary>
    public interface IExamManager
    {
        /// <summary>
        /// Loads questions set from requested task
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Response<ExamAttempt>> AttemptGet(Request<ExamTask> request);

        Task<Response<ExamAttempt>> AttemptPost(Request<ExamAttempt> request);

        Task<Response<ExamAttempt>> Results(Request<int> request);

        Task<Response<ExamAttempt>> DeletePreview(Request<int> request);

        Task<Response<bool>> Delete(Request<int> request);
    }
}
