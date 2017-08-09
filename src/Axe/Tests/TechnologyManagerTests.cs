using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Axe.Models;
using Axe.Models.TechnologiesVm;
using Axe.Managers;

namespace Axe.Tests
{
    [TestFixture]
    public class TechnologyManagerTests
    {
        private AxeDbContext db;

        private Technology techA, techB, techC;
        private ApplicationUser expertA, expertB, expertC;

        private ITechnologyManager manager;

        [OneTimeSetUp]
        public void InitTestFixture()
        {
            var options = new DbContextOptionsBuilder<AxeDbContext>()
                .UseInMemoryDatabase(databaseName: "TechManagerDb")
                .Options;

            this.db = new AxeDbContext(options);

            this.expertA = new ApplicationUser { UserName = "A" };
            this.expertB = new ApplicationUser { UserName = "B" };
            this.expertC = new ApplicationUser { UserName = "C" };

            this.techA = new Technology { Name = "A", InformationText = "A" };
            this.techB = new Technology { Name = "B", InformationText = "B" };
            this.techC = new Technology { Name = "C", InformationText = "C" };

            this.db.AddRange
                (
                    this.expertA, this.expertB, this.expertC, 
                    this.techA, this.techB, this.techC,
                    new ExpertTechnologyLink { Technology = techA, User = expertA, },
                    new ExpertTechnologyLink { Technology = techB, User = expertB, },
                    new ExpertTechnologyLink { Technology = techC, User = expertC, }
                );
            this.db.SaveChanges();

            this.manager = new TechnologyManager(this.db);
        }

        [SetUp]
        public void InitTestCase()
        {
        }

        private Request<T> Request<T>(T item, ApplicationUser user = null)
        {
            return new Request<T> { Item = item, CurrentUser = user, ModelState = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary() };
        }

        [TestCase]
        public async Task TechnologyCreate_Success()
        {
            int count = this.db.Technology.Count();

            var tech = new Technology { Name = "Z", InformationText = "Z" };

            var request = Request(tech, this.expertA);

            var response = await this.manager.InputPost(request);

            Assert.AreEqual(ResponseCode.Success, response.Code);

            var techRecord = this.db.Technology.Include(t => t.Experts)
                .SingleOrDefault(t => t.Name == tech.Name);

            Assert.NotNull(techRecord);
            Assert.True(techRecord.Experts.Count == 1 && techRecord.Experts.First().UserId == this.expertA.Id);

            Assert.AreEqual(count + 1, this.db.Technology.Count());

            this.db.Remove(techRecord);
            await this.db.SaveChangesAsync();
        }

        [TestCase]
        public async Task TechnologyCreate_DuplicateName()
        {
            int count = this.db.Technology.Count();

            var tech = new Technology { Name = "A", InformationText = "A" };

            var request = Request(tech, this.expertA);

            var response = await this.manager.InputPost(request);

            Assert.AreEqual(ResponseCode.ValidationError, response.Code);
            Assert.AreEqual(0, tech.Id);
            Assert.False(request.ModelState.IsValid);
            Assert.AreEqual(count, this.db.Technology.Count());
        }

        [TestCase]
        public async Task TechnologyCreate_WrongId()
        {
            int count = this.db.Technology.Count();

            var tech = new Technology { Id = 100, Name = "Q", InformationText = "Q" };

            var request = Request(tech, this.expertA);            

            var response = await this.manager.InputPost(request);

            Assert.AreEqual(ResponseCode.NotFound, response.Code);
            Assert.AreEqual(count, this.db.Technology.Count());
        }

        [TestCase]
        public async Task TechnologyIndex_ExpertAccess()
        {
            var requestA = this.Request<int?>(null, expertA);
            var responseA = await this.manager.Index(requestA);
            var vm = responseA.Item;

            Assert.AreEqual(ResponseCode.Success, responseA.Code);
            Assert.True(vm.Technologies.Count == 1 && vm.Technologies[0].Name == techA.Name);
            Assert.True(vm.SelectedTechnology.Name == techA.Name);
            Assert.NotNull(vm.Questions);
            Assert.NotNull(vm.Exams);
            Assert.NotNull(vm.Experts);            
        }
    }
}
