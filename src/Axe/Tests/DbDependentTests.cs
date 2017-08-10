using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Axe.Models;
using Axe.Managers;

namespace Axe.Tests
{
    /// <summary>
    /// Class contains basic operations for db dependent tests 
    /// </summary>
    public abstract class DbDependentTests
    {
        protected DbContextOptions<AxeDbContext> dbOptions;       
        protected AxeDbContext db;

        protected Technology techA, techB, techC;
        protected ApplicationUser expertA, expertB, expertC;

        /// <summary>
        /// Creates in-memory storage
        /// </summary>
        /// <param name="st">Storage identifier</param>
        protected virtual void InitStorage(string storageName)
        {
            dbOptions = new DbContextOptionsBuilder<AxeDbContext>()
                        .UseInMemoryDatabase(storageName)
                        .Options;
        }

        /// <summary>
        /// Initializes test Technology and User entities
        /// </summary>
        protected virtual void InitTechnologies()
        {
            this.expertA = new ApplicationUser { UserName = "A" };
            this.expertB = new ApplicationUser { UserName = "B" };
            this.expertC = new ApplicationUser { UserName = "C" };

            this.techA = new Technology { Name = "A", InformationText = "A" };
            this.techB = new Technology { Name = "B", InformationText = "B" };
            this.techC = new Technology { Name = "C", InformationText = "C" };

            using (var context = NewDbContext())
            {
                context.AddRange
                (
                    this.expertA, this.expertB, this.expertC,
                    this.techA, this.techB, this.techC,
                    new ExpertTechnologyLink { Technology = this.techA, User = this.expertA, },
                    new ExpertTechnologyLink { Technology = this.techB, User = this.expertB, },
                    new ExpertTechnologyLink { Technology = this.techC, User = this.expertC, }
                );
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Creates new connection to storage
        /// </summary>
        /// <returns></returns>
        protected AxeDbContext NewDbContext()
        {
            return new AxeDbContext(dbOptions);
        }

        protected Request<T> Request<T>(T item, ApplicationUser user = null)
        {
            return new Request<T> { Item = item, CurrentUser = user, ModelState = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary() };
        }
    }
}
