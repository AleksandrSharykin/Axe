using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Axe.Models;
using Axe.Managers;
using NUnit.Framework;

namespace Axe.Tests
{
    [TestFixture]
    public class HomeManagerTests
    {
        private DbContextOptions<AxeDbContext> dbOptions;

        private AxeDbContext db;
        private AxeDbContext dbManager;
        private IHomeManager manager;

        [OneTimeSetUp]
        public void InitTestFixture()
        {
            this.dbOptions = new DbContextOptionsBuilder<AxeDbContext>()
                .UseInMemoryDatabase("HomeManagerDb")
                .Options;
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
            this.manager = new HomeManager(this.dbManager);
        }

        [TearDown]
        public void ClearTestCase()
        {
            this.dbManager.Dispose();
            this.db.Dispose();
        }

        [TestCase]
        public async Task HomeIndex_Empty()
        {
            var response = await this.manager.Index();
            Assert.AreEqual(ResponseCode.Success, response.Code);
            Assert.AreEqual(0, response.Item.Count);
        }

        [TestCase]
        public async Task HomeIndex_One()
        {
            var tech = new Technology { Name = "A", InformationText = "A" };
            this.db.Add(tech);
            this.db.SaveChanges();

            var response = await this.manager.Index();
            Assert.AreEqual(ResponseCode.Success, response.Code);
            Assert.AreEqual(1, response.Item.Count);
            Assert.AreEqual("A", response.Item[0].Name);

            this.db.Remove(tech);
            this.db.SaveChanges();
        }

        [TestCase]
        public async Task HomeIndex_Many()
        {
            var techA = new Technology { Name = "A", InformationText = "A" };
            var techB = new Technology { Name = "B", InformationText = "B" };
            this.db.AddRange(techA, techB);
            this.db.SaveChanges();

            var response = await this.manager.Index();
            Assert.AreEqual(ResponseCode.Success, response.Code);
            Assert.AreEqual(2, response.Item.Count);

            this.db.Technology.RemoveRange(techA, techB);
            this.db.SaveChanges();
        }
    }
}
