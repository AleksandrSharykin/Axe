using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Axe.Models
{
    public class AxeDbContext : IdentityDbContext<ApplicationUser>
    {
        public AxeDbContext (DbContextOptions<AxeDbContext> options)
            : base(options)
        {            
        }

        public DbSet<Axe.Models.Technology> Technology { get; set; }        
    }
}
