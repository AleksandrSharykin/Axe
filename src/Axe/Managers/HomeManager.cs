using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Axe.Models;

namespace Axe.Managers
{
    /// <summary>
    /// Class implements operations governed by HomeController
    /// </summary>
    public class HomeManager: ManagerBase, IHomeManager
    {        
        public HomeManager(AxeDbContext context) : base(context) { }

        /// <summary>
        /// Get a list of technologies to display on home page
        /// </summary>
        /// <returns></returns>
        public async Task<Response<IList<Technology>>> Index()
        {
            IList<Technology> techs = await this.context.Technology.ToListAsync();
            return this.Response(techs);
        }
    }
}
