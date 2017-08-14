using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Axe.Models;
using Axe.Models.AssessmentsVm;
using Axe.Managers;

namespace Axe.Tests
{
    [TestFixture]
    public class AssessmentManagerTests : DbDependentTests
    {
        private AxeDbContext dbManager;
        private IAssessmentManager manager;

        #region Setup
        [OneTimeSetUp]
        public void InitTestFixture()
        {
            this.InitStorage("SkillManagerDb");
            this.InitTechnologies();
        }

        [SetUp]
        public void InitTestCase()
        {
            this.db = NewDbContext();

            this.dbManager = NewDbContext();
            this.manager = new AssessmentManager(this.dbManager);
        }

        [TearDown]
        public void ClearTestCase()
        {
            this.dbManager.Dispose();
            this.db.Dispose();
        }

        #endregion
    }
}
