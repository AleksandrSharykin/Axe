using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Axe.Models;
using Axe.Models.TechnologiesVm;

namespace Axe.Managers
{
    /// <summary>
    /// Class implements operations which can be performed with <see cref="Technology"/> entities
    /// </summary>
    public class TechnologyManager : ManagerBase, ITechnologyManager
    {
        public TechnologyManager(AxeDbContext context) : base(context) { }

        /// <summary>
        /// Returns a list of technologies available for current user with details about selected technology
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Response<TechnologiesIndexVm>> Index(Request<int?> request)
        {
            int? technologyId = request.Item;
            // get currrent user with list of technologies where they are an expert
            var user = await this.context.Users.Include(u => u.Technologies).ThenInclude(x => x.Technology)
                             .SingleAsync(u => u.Id == request.CurrentUser.Id);

            // create tech list for selection
            var techs = user.Technologies.Select(x => x.Technology).ToList();

            var selectedTech = techs.FirstOrDefault(t => t.Id == technologyId) ??
                               techs.FirstOrDefault();
            technologyId = selectedTech?.Id;

            // prepare list of users to assign as experts in selected technology
            var experts = new List<ExpertSelectionVm>();
            if (selectedTech != null)
            {
                selectedTech = this.context.Technology.Include(t => t.Experts)
                                   .SingleOrDefault(t => t.Id == technologyId);

                var expertsIds = selectedTech.Experts.Select(u => u.UserId).ToList();
                experts = this.context.Users
                              .Where(u => u.Id != user.Id)
                              .Select(u => new ExpertSelectionVm
                              {
                                  Id = u.Id,
                                  UserName = u.UserName,
                                  Email = u.Email,
                                  IsExpert = expertsIds.Contains(u.Id),
                              })
                              .ToList();
            }

            var vm = new TechnologiesIndexVm
            {
                Technologies = techs,
                SelectedTechnology = selectedTech,

                Questions = this.context.TaskQuestion
                                .Include(q => q.Author)
                                .Where(q => q.TechnologyId == technologyId)
                                .ToList(),

                Exams = this.context.ExamTask.Include(t => t.Author)
                                .Include(t => t.Questions)
                                .Where(t => t.TechnologyId == technologyId)
                                .ToList(),

                Experts = experts
            };

            return this.Response(vm);
        }

        /// <summary>
        /// Gets <see cref="Technology"/> template for creation or edit
        /// </summary>
        public async Task<Response<Technology>> InputGet(Request<int?> request)
        {
            int? id = request.Item;

            // creating a new Technology
            if (id == null)
            {
                return this.New<Technology>();
            }

            var technology = await this.context.Technology.Include(t => t.Experts)
                                               .SingleOrDefaultAsync(m => m.Id == id);

            // creating a new Technology if requested identifier was not found
            if (technology == null)
            {
                return this.New<Technology>();
            }

            // if User is not an Expert they cannot edit Technology
            if (false == technology.Experts.Any(t => t.UserId == request.CurrentUser.Id))
            {
                return NotFound<Technology>();
            }

            return this.Response(technology);
        }

        /// <summary>
        /// Applies <see cref="Technology"/> edit results
        /// </summary>
        public async Task<Response<Technology>> InputPost(Request<Technology> request)
        {
            var user = request.CurrentUser;

            var technologyInput = request.Item;

            int id = technologyInput.Id;

            if (id > 0)
            {
                // edit
                var technology = await this.context.Technology.Include(t => t.Experts)
                                           .AsNoTracking()
                                           .SingleOrDefaultAsync(m => m.Id == id);

                if (technology == null)
                {
                    return this.NotFound<Technology>();
                }

                // duplicate names are not allowed
                if (this.context.Technology.Any(t => t.Name == technologyInput.Name && t.Id != id))
                {
                    request.ModelState.AddModelError(String.Empty, technologyInput.Name + " technology is already exists");
                    return this.ValidationError(technologyInput);
                }

                // if User is not an Expert they cannot edit Technology
                if (false == technology.Experts.Any(t => t.UserId == user.Id))
                {
                    request.ModelState.AddModelError(String.Empty, "Only experts can edit technology");
                    return this.ValidationError(technologyInput);
                }

                this.context.Update(technologyInput);
            }
            else
            {
                // add

                // duplicate names are not allowed
                if (this.context.Technology.Any(t => t.Name == technologyInput.Name))
                {
                    request.ModelState.AddModelError(String.Empty, technologyInput.Name + " technology is already exists");
                }

                if (request.ModelState.IsValid)
                {
                    this.context.Add(technologyInput);

                    // technology creator becomes an Expert
                    this.context.Add(new ExpertTechnologyLink { User = user, Technology = technologyInput });
                }
            }

            if (request.ModelState.IsValid)
            {
                await this.context.SaveChangesAsync();
                return this.Response(technologyInput);
            }

            return this.ValidationError(technologyInput);
        }

        /// <summary>
        /// Adds a user to technology expert list
        /// </summary>
        public async Task IncludeExpert(Request<ExpertTechnologyLink> request)
        {
            await SetExpert(request, true);
        }

        /// <summary>
        /// Removes a user from technology experts list
        /// </summary>
        public async Task ExcludeExpert(Request<ExpertTechnologyLink> request)
        {
            await SetExpert(request, false);
        }

        /// <summary>
        /// Adds or Removes a user from technology experts list
        /// </summary>
        /// <param name="include">Indicator of operation (true = add, false = remove)</param>
        private async Task SetExpert(Request<ExpertTechnologyLink> request, bool include)
        {
            string id = request.Item.UserId;
            int technologyId = request.Item.TechnologyId;

            var tech = await this.context.Technology.Include(t => t.Experts).SingleOrDefaultAsync(t => t.Id == technologyId);
            if (tech == null)
            {
                // requested tech is not found
                return;
            }

            var currentUser = request.CurrentUser;
            if (false == tech.Experts.Any(x => x.UserId == currentUser.Id))
            {
                // current user is not an expert in selected tech
                return;
            }

            if (false == this.context.Users.Any(u => u.Id == id))
            {
                // requested user is not found
                return;
            }

            if (include && false == tech.Experts.Any(u => u.UserId == id))
            {
                // include selected User in Experts in requested Technology
                this.context.Add(new ExpertTechnologyLink { UserId = id, TechnologyId = technologyId });
                this.context.SaveChanges();
            }
            else if (false == include)
            {
                var expert = tech.Experts.FirstOrDefault(t => t.UserId == id);
                if (expert != null)
                {
                    // exclude selected User from Experts in requested Technology
                    this.context.Remove(expert);
                    await this.context.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Gets <see cref="Technology"/> for preview before deletion
        /// </summary>
        public async Task<Response<Technology>> DeleteGet(Request<int> request)
        {
            var tech = await this.context.Technology.Include(t => t.Experts)
                                 .SingleOrDefaultAsync(t => t.Id == request.Item);

            if (tech == null || false == tech.Experts.Any(u => u.UserId == request.CurrentUser.Id))
            {
                return this.NotFound<Technology>();
            }

            return this.Response(tech);
        }

        /// <summary>
        /// Deletes <see cref="Technology"/>
        /// </summary>
        public async Task<Response<Technology>> DeletePost(Request<int> request)
        {
            var tech = await this.context.Technology.Include(t => t.Experts)
                                 .SingleOrDefaultAsync(t => t.Id == request.Item);

            if (tech == null)
            {
                return this.NotFound<Technology>();
            }

            if (false == tech.Experts.Any(u => u.UserId == request.CurrentUser.Id))
            {
                request.ModelState.AddModelError(String.Empty, "Only expert can delete " + tech.Name);
                return this.ValidationError(tech);
            }

            this.context.Remove(tech);
            await this.context.SaveChangesAsync();

            return this.Response(new Technology());
        }
    }
}
