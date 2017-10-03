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
    public class HomeManager : ManagerBase, IHomeManager
    {
        public HomeManager(AxeDbContext context) : base(context) { }
        
        public async Task<Response<IList<ApplicationUser>>> GetAdministrators(Request<int> request)
        {
            var role = this.context.Roles.FirstOrDefault(r => r.Name == UserRole.Superuser);

            List<string> userIds = await context.UserRoles.Where(a => a.RoleId == role.Id).Select(b => b.UserId).Distinct().ToListAsync();
            //The first step: get all user id collection as userids based on role from db.UserRoles
            IList<ApplicationUser> admins = await context.Users.Where(a => userIds.Any(c => c == a.Id)).ToListAsync();
            //The second step: find all users collection from _db.Users which 's Id is contained at userids ;

            return this.Response(admins);
        }
        
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
