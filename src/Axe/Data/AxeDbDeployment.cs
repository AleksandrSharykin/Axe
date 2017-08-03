using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Axe.Models
{
    public static class AxeDbDeployment
    {
        public static async Task Deploy(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, AxeDbContext context)
        {
            context.Database.EnsureCreated();
            
            if (context.Roles.Any())
            {
                // db has been configurated already
                return;  
            }
            
            await roleManager.CreateAsync(new IdentityRole(UserRole.Superuser));            
            await roleManager.CreateAsync(new IdentityRole(UserRole.Member));

            var superuser = new ApplicationUser
            {                
                UserName = "superuser@supermail.com",
                Email = "superuser@supermail.com",
                JobPosition = "admin",
            };

            await userManager.CreateAsync(superuser, "T0pSecret");
            await userManager.AddToRolesAsync(superuser, new string[] { UserRole.Superuser, UserRole.Member });

            if (context.Technology.Any())
            {
                // data has been added already
                return;
            }

            var csharp = new Technology
            {
                Name = "C#",
                InformationText =
@"C# is a programming language that is designed for building a variety of applications that run on the .NET Framework.
C# is simple, powerful, type-safe, and object-oriented",
            };
            csharp.Experts = new List<ExpertTechnologyLink> { new ExpertTechnologyLink { User = superuser, Technology = csharp } };

            var javascript = new Technology
            {
                Name="JavaScript",
                InformationText =
@"JavaScript is a high-level, dynamic, multi-paradigm, object-oriented, prototype-based, weakly-typed language traditionally used for client-side scripting in web browsers.",
            };
            javascript.Experts = new List<ExpertTechnologyLink> { new ExpertTechnologyLink { User = superuser, Technology = javascript } };

            context.AddRange(csharp, javascript);

            await context.SaveChangesAsync();
        }
    }
}
