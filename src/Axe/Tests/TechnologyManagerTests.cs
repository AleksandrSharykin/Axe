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
        private DbContextOptions<AxeDbContext> dbOptions;
        private AxeDbContext dbManager;
        private AxeDbContext db;

        private Technology techA, techB, techC;
        private ApplicationUser expertA, expertB, expertC;

        private ITechnologyManager manager;

        [OneTimeSetUp]        
        public void InitTestFixture()
        {
            dbOptions = new DbContextOptionsBuilder<AxeDbContext>()
                .UseInMemoryDatabase("TechManagerDb")
                .Options;                      

            this.expertA = new ApplicationUser { UserName = "A" };
            this.expertB = new ApplicationUser { UserName = "B" };
            this.expertC = new ApplicationUser { UserName = "C" };

            this.techA = new Technology { Name = "A", InformationText = "A" };
            this.techB = new Technology { Name = "B", InformationText = "B" };
            this.techC = new Technology { Name = "C", InformationText = "C" };

            using (var db = NewDbContext())
            {
                db.AddRange
                    (
                        this.expertA, this.expertB, this.expertC,
                        this.techA, this.techB, this.techC,
                        new ExpertTechnologyLink { Technology = techA, User = expertA, },
                        new ExpertTechnologyLink { Technology = techB, User = expertB, },
                        new ExpertTechnologyLink { Technology = techC, User = expertC, }
                    );
                db.SaveChanges();
            }            
        }        

        private AxeDbContext NewDbContext()
        {
            return new AxeDbContext(dbOptions);
        }

        [SetUp]        
        public void InitTestCase()
        {
            this.db = NewDbContext();

            this.dbManager = NewDbContext();
            this.manager = new TechnologyManager(this.dbManager);            
        }

        [TearDown]
        public void ClearTestCase()
        {
            this.dbManager.Dispose();
            this.db.Dispose();
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

            var request = Request(tech, this.dbManager.Users.Single(u => u.Id == expertA.Id));

            var response = await this.manager.InputPost(request);

            Assert.AreEqual(ResponseCode.Success, response.Code);

            var techRecord = this.db.Technology.Include(t => t.Experts)
                                        .SingleOrDefault(t => t.Name == tech.Name);

            Assert.NotNull(techRecord);
            Assert.True(techRecord.Experts.Count == 1 && techRecord.Experts.First().UserId == this.expertA.Id);

            Assert.AreEqual(count + 1, this.db.Technology.Count());

            this.db.Remove(techRecord);
            this.db.SaveChanges();
        }

        /// <summary>
        /// Attemps to create a technology with a name which is already exists
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Attempts to create a technology with some Id
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Expert requests index data
        /// </summary>
        /// <returns></returns>
        [TestCase]
        public async Task TechnologyIndex_ExpertAccess()
        {
            var requestA = this.Request<int?>(null, expertA);
            var responseA = await this.manager.Index(requestA);
            var vm = responseA.Item;

            Assert.AreEqual(ResponseCode.Success, responseA.Code);
            Assert.True(vm.Technologies.Count == 1 && vm.Technologies[0].Id == techA.Id);
            Assert.True(vm.SelectedTechnology.Id == techA.Id);            
            Assert.True(vm.Questions.Count == 0);
            Assert.True(vm.Exams.Count == 0);
            Assert.True(vm.Experts.Count == 2 && vm.Experts.Count(x => x.Id == expertB.Id) == 1 && vm.Experts.Count(x => x.Id == expertC.Id) == 1);
        }

        /// <summary>
        /// Expert requests index data for a not existing technology
        /// </summary>
        /// <returns></returns>
        [TestCase]
        public async Task TechnologyIndex_ExpertAccessWrongId()
        {
            var requestA = this.Request<int?>(314, expertA);
            var responseA = await this.manager.Index(requestA);
            var vm = responseA.Item;

            Assert.AreEqual(ResponseCode.Success, responseA.Code);
            Assert.True(vm.Technologies.Count == 1 && vm.Technologies[0].Id == techA.Id);
            Assert.True(vm.SelectedTechnology.Id == techA.Id);
            Assert.True(vm.Questions.Count == 0);
            Assert.True(vm.Exams.Count == 0);
            Assert.True(vm.Experts.Count == 2 && vm.Experts.Count(x => x.Id == expertB.Id) == 1 && vm.Experts.Count(x => x.Id == expertC.Id) == 1);
        }

        /// <summary>
        /// User requests index data for a technology where trey are not expert
        /// </summary>
        /// <returns></returns>
        [TestCase]
        public async Task TechnologyIndex_NonExpertAcces()
        {
            var requestA = this.Request<int?>(techB.Id, expertA);
            var responseA = await this.manager.Index(requestA);
            var vm = responseA.Item;

            Assert.AreEqual(ResponseCode.Success, responseA.Code);
            Assert.True(vm.Technologies.Count == 1 && vm.Technologies[0].Id == techA.Id);
            Assert.True(vm.SelectedTechnology.Id == techA.Id);
            Assert.True(vm.Questions.Count == 0);
            Assert.True(vm.Exams.Count == 0);
            Assert.True(vm.Experts.Count == 2 && vm.Experts.Count(x => x.Id == expertB.Id) == 1 && vm.Experts.Count(x => x.Id == expertC.Id) == 1);
        }

        /// <summary>
        /// User create a new technology and requests index data for it
        /// </summary>
        /// <returns></returns>
        [TestCase]
        public async Task TechnologyIndex_ExpertAccessAfterCreate()
        {
            var tech = new Technology { Name = "Z", InformationText = "Z" };

            var requestCreate = Request(tech, this.dbManager.Users.Single(u => u.Id == this.expertC.Id));

            var responseCreate = await this.manager.InputPost(requestCreate);

            Assert.AreEqual(ResponseCode.Success, responseCreate.Code);

            var requestIndex = this.Request<int?>(tech.Id, expertC);
            var responseIndex = await this.manager.Index(requestIndex);
            var vm = responseIndex.Item;

            Assert.AreEqual(ResponseCode.Success, responseIndex.Code);
            Assert.True(vm.Technologies.Count == 2);
            Assert.True(vm.SelectedTechnology.Id == tech.Id);

            Assert.True(vm.Questions.Count == 0);
            Assert.True(vm.Exams.Count == 0);
            Assert.True(vm.Experts.Count == 2 && vm.Experts.Count(x => x.Id == expertA.Id) == 1 && vm.Experts.Count(x => x.Id == expertA.Id) == 1);

            this.db.Remove(tech);
            this.db.SaveChanges();
        }

        [TestCase]
        public async Task TechnologyEdit_InputGetNew()
        {
            var request = this.Request<int?>(null, this.expertA);

            var response = await this.manager.InputGet(request);

            var tech = response.Item;

            Assert.AreEqual(ResponseCode.Success, response.Code);
            Assert.NotNull(tech);
            Assert.AreEqual(0, tech.Id);
            Assert.AreEqual(null, tech.Name);
        }

        [TestCase]
        public async Task TechnologyEdit_InputGetWrongId()
        {
            var request = this.Request<int?>(314, this.expertA);

            var response = await this.manager.InputGet(request);

            var tech = response.Item;

            Assert.AreEqual(ResponseCode.Success, response.Code);
            Assert.NotNull(tech);
            Assert.AreEqual(0, tech.Id);
            Assert.AreEqual(null, tech.Name);
        }

        [TestCase]
        public async Task TechnologyEdit_InputGetSuccess()
        {
            var request = this.Request<int?>(techA.Id, this.expertA);

            var response = await this.manager.InputGet(request);

            var tech = response.Item;

            Assert.AreEqual(ResponseCode.Success, response.Code);
            Assert.NotNull(tech);
            Assert.AreEqual(techA.Id, tech.Id);
            Assert.AreEqual(techA.Name, tech.Name);
        }

        [TestCase]
        public async Task TechnologyEdit_InputGetNonExpert()
        {
            var request = this.Request<int?>(techB.Id, this.expertA);

            var response = await this.manager.InputGet(request);            

            Assert.AreEqual(ResponseCode.NotFound, response.Code);
            Assert.Null(response.Item);            
        }

        /// <summary>
        /// Experts changes technology information text successfully
        /// </summary>
        /// <returns></returns>
        [TestCase]
        public async Task TechnologyEdit_InputPostSuccess()
        {
            int count = this.db.Technology.Count();

            var tech = this.dbManager.Technology.Single(t => t.Id == techA.Id);

            tech.InformationText = tech.InformationText + "!";

            var request = Request(tech, this.expertA);

            var response = await this.manager.InputPost(request);

            var techEdited = this.db.Technology.Single(t => t.Id == techA.Id);

            Assert.AreEqual(ResponseCode.Success, response.Code);
            Assert.NotNull(techEdited);
            Assert.AreEqual(techA.InformationText + "!", techEdited.InformationText);
            Assert.False(ReferenceEquals(tech, techEdited));
            Assert.AreEqual(count, this.db.Technology.Count());
        }

        /// <summary>
        /// Experts attemps to set duplicate name unsuccessfully
        /// </summary>
        /// <returns></returns>
        [TestCase]
        public async Task TechnologyEdit_InputPostDuplicateName()
        {
            int count = this.db.Technology.Count();

            var tech = this.dbManager.Technology.Single(t => t.Id == this.techA.Id);
            tech.Name = techC.Name;

            var request = Request(tech, this.expertA);

            var response = await this.manager.InputPost(request);
            
            var techEdited = this.db.Technology.Single(t => t.Id == techA.Id);
            
            Assert.AreEqual(ResponseCode.ValidationError, response.Code);
            Assert.AreEqual(techA.Name, techEdited.Name);
            Assert.AreEqual(count, this.db.Technology.Count());
        }

        /// <summary>
        /// User tries to change technology without expert access
        /// </summary>
        /// <returns></returns>
        [TestCase]
        public async Task TechnologyEdit_InputPostNonExpert()
        {
            var tech = new Technology { Id = techC.Id, Name = techC.Name, InformationText = techC.InformationText + "!" };

            var request = Request(tech, this.expertA);

            var response = await this.manager.InputPost(request);

            var techEdited = this.db.Technology.Single(t => t.Id == techC.Id);
            
            Assert.AreEqual(ResponseCode.ValidationError, response.Code);
            Assert.AreEqual(techC.InformationText, techEdited.InformationText);            
        }

        [TestCase]
        public async Task TechnologyIncludeExclude_Expert()
        {
            var expertAssignment = new ExpertTechnologyLink { UserId = this.expertB.Id, TechnologyId = this.techA.Id };

            var request = this.Request(expertAssignment, this.expertA);

            await this.manager.IncludeExpert(request);

            var tech = this.db.Technology.Include(t => t.Experts).Single(t => t.Id == this.techA.Id);

            Assert.IsTrue(tech.Experts.Count(u => u.UserId == this.expertB.Id) == 1);

            await this.manager.ExcludeExpert(request);
            
            using (var dbConfirm = NewDbContext())
            {
                tech = dbConfirm.Technology.Include(t => t.Experts).Single(t => t.Id == this.techA.Id);
            }
            Assert.IsTrue(tech.Experts.Count(u => u.UserId == this.expertB.Id) == 0);
        }

        [TestCase]
        public async Task TechnologyInclude_NotExpert()
        {
            var expertAssignment = new ExpertTechnologyLink { UserId = this.expertB.Id, TechnologyId = this.techA.Id };

            var request = this.Request(expertAssignment, this.expertC);

            await this.manager.IncludeExpert(request);

            var tech = this.db.Technology.Include(t => t.Experts).Single(t => t.Id == this.techA.Id);

            Assert.IsTrue(tech.Experts.Count(u => u.UserId == this.expertB.Id) == 0);
        }

        [TestCase]
        public async Task TechnologyExclude_NotExpert()
        {
            var expertAssignment = new ExpertTechnologyLink { UserId = this.expertA.Id, TechnologyId = this.techA.Id };

            var request = this.Request(expertAssignment, this.expertC);

            await this.manager.ExcludeExpert(request);

            var tech = this.db.Technology.Include(t => t.Experts).Single(t => t.Id == this.techA.Id);

            Assert.IsTrue(tech.Experts.Count(u => u.UserId == this.expertA.Id) == 1);
        }

        [TestCase]
        public async Task TechnologyExclude_NotExisted()
        {
            var expertAssignment = new ExpertTechnologyLink { UserId = this.expertB.Id, TechnologyId = this.techA.Id };

            var request = this.Request(expertAssignment, this.expertA);

            await this.manager.ExcludeExpert(request);

            var tech = this.db.Technology.Include(t => t.Experts).Single(t => t.Id == this.techA.Id);

            Assert.AreEqual(1, tech.Experts.Count());
            Assert.IsTrue(tech.Experts.Count(u => u.UserId == this.expertA.Id) == 1);
            Assert.IsTrue(tech.Experts.Count(u => u.UserId == this.expertB.Id) == 0);
        }
    }
}
