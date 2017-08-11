using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Axe.Models;
using Axe.Models.TechnologiesVm;

namespace Axe.Managers
{
    /// <summary>
    /// Interface declares operations which can be performed with <see cref="Technology"/> entities
    /// </summary>
    public interface ITechnologyManager
    {
        /// <summary>
        /// Returns a list of technologies available for current user with details about selected technology
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Response<TechnologiesIndexVm>> Index(Request<int?> request);

        /// <summary>
        /// Gets <see cref="Technology"/> template for creation or edit
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Response<Technology>> InputGet(Request<int?> request);

        /// <summary>
        /// Applies <see cref="Technology"/> edit results
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Response<Technology>> InputPost(Request<Technology> request);

        /// <summary>
        /// Adds a user to technology expert list
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task IncludeExpert(Request<ExpertTechnologyLink> request);

        /// <summary>
        /// Removes a user from technology experts list
        /// </summary>
        Task ExcludeExpert(Request<ExpertTechnologyLink> request);

        /// <summary>
        /// Gets <see cref="Technology"/> for preview before deletion
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Response<Technology>> DeleteGet(Request<int> request);

        /// <summary>
        /// Deletes <see cref="Technology"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Response<Technology>> DeletePost(Request<int> request);
    }
}
