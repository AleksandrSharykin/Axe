using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Axe.Models;

namespace Axe.Managers
{
    /// <summary>
    /// Base class for managers with basic functionality
    /// </summary>
    public class ManagerBase
    {
        /// <summary>
        /// Shortcut to return generic response with data
        /// </summary>
        /// <typeparam name="Tout"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        protected Response<Tout> Response<Tout>(Tout item)
        {
            return new Response<Tout>(item);
        }

        /// <summary>
        /// Shortcut to return <see cref="ResponseCode.ValidationError"/> response
        /// </summary>
        /// <typeparam name="Tout"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        protected Response<Tout> ValidationError<Tout>(Tout item)
        {
            return new Response<Tout>(item) { Code = ResponseCode.ValidationError };
        }

        /// <summary>
        /// Shortcut to return <see cref="ResponseCode.NotFound"/> response
        /// </summary>
        protected Response<Tout> NotFound<Tout>()
        {
            return new Response<Tout> { Code = ResponseCode.NotFound };
        }

        /// <summary>
        /// Shortcut to return generic response with default data template
        /// </summary>
        /// <typeparam name="Tout"></typeparam>
        /// <returns></returns>
        protected Response<Tout> New<Tout>() where Tout: new()
        {
            return new Response<Tout> { Item = new Tout() };
        }
    }
}
