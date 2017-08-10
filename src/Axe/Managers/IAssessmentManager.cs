using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Axe.Models;
using Axe.Models.AssessmentsVm;

namespace Axe.Managers
{
    /// <summary>
    /// Interface declares operations governed by AssessmentController
    /// </summary>
    public interface IAssessmentManager
    {
        /// <summary>
        /// Gets <see cref="SkillAssessment"/> object template for creation or edit
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Response<AssessmentInputVm>> InputGet(Request<SkillAssessment> request);

        /// <summary>
        /// Applies <see cref="SkillAssessment"/> edit results
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Response<AssessmentInputVm>> InputPost(Request<AssessmentInputVm> request);

        /// <summary>
        /// Gets <see cref="SkillAssessment"/> details for display
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Response<AssessmentDetailsVm>> DetailsGet(Request<int> request);

        /// <summary>
        /// Gets <see cref="SkillAssessment"/> for marking by examiner 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Response<AssessmentDetailsVm>> MarkGet(Request<int> request);

        /// <summary>
        /// Applies <see cref="SkillAssessment"/> mark results
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Response<AssessmentDetailsVm>> MarkPost(Request<AssessmentDetailsVm> request);
    }
}
