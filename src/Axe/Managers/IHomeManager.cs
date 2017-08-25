using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Axe.Models;

namespace Axe.Managers
{
    /// <summary>
    /// Interface declares operations governed by HomeController
    /// </summary>
    public interface IHomeManager
    {
        Task<Response<IList<Technology>>> Index();

        Task<Response<IList<ApplicationUser>>> GetAdministrators(Request<int> request);
    }
}
