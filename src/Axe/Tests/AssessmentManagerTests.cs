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

        /// <summary>
        /// Verifies that required input properties are set
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        private bool IsAssessmentDataComplete(AssessmentInputVm vm)
        {
            return String.IsNullOrWhiteSpace(vm.TechnologyName) == false && vm.TechnologyId > 0 &&
                   String.IsNullOrWhiteSpace(vm.StudentName) == false && String.IsNullOrWhiteSpace(vm.StudentId) == false &&
                   String.IsNullOrWhiteSpace(vm.ExaminerName) == false && String.IsNullOrWhiteSpace(vm.ExaminerId) == false;
        }

        /// <summary>
        /// Verifies that required display properties are set
        /// </summary>
        private bool IsAssessmentDataComplete(AssessmentDetailsVm vm)
        {
            return vm.Technology != null &&
                   vm.Student != null &&
                   vm.Examiner != null;
        }

        [TestCase]
        public async Task AssessmentGet_WrongId()
        {
            var request = this.Request(new SkillAssessment { Id = 100, TechnologyId = this.techA.Id, StudentId = this.expertB.Id }, this.expertA);
            var response = await this.manager.InputGet(request);

            Assert.AreEqual(ResponseCode.Success, response.Code);
            Assert.NotNull(response.Item);
            Assert.AreEqual(0, response.Item.Id);
            Assert.True(this.IsAssessmentDataComplete(response.Item));

            int count = this.db.SkillAssessment.Count();
            Assert.AreEqual(0, count);
        }

        [TestCase]
        public async Task AssessmentGet_NonExpert()
        {
            var request = this.Request(new SkillAssessment { TechnologyId = this.techA.Id, StudentId = this.expertB.Id }, this.expertC);
            var response = await this.manager.InputGet(request);

            Assert.AreEqual(ResponseCode.NotFound, response.Code);
            Assert.Null(response.Item);

            int count = this.db.SkillAssessment.Count();
            Assert.AreEqual(0, count);
        }

        [TestCase]
        public async Task AssessmentGet_SelfAssessmentFailed()
        {
            var request = this.Request(new SkillAssessment { TechnologyId = this.techA.Id, StudentId = this.expertA.Id }, this.expertA);
            var response = await this.manager.InputGet(request);

            Assert.AreEqual(ResponseCode.NotFound, response.Code);
            Assert.Null(response.Item);

            int count = this.db.SkillAssessment.Count();
            Assert.AreEqual(0, count);
        }

        [TestCase]
        public async Task AssessmentPost_CreateSuccess()
        {
            var dt = new DateTime(2017, 12, 31, 14, 15, 0);
            var input = new AssessmentInputVm
            {
                TechnologyId = this.techA.Id,
                StudentId = this.expertB.Id,
                ExaminerId = this.expertA.Id,
                ExamDate = dt.Date,
                ExamTime = dt,
            };

            int? id = null;
            try
            {
                var request = this.Request(input, this.expertA);

                var response = await this.manager.InputPost(request);

                var item = response.Item;
                Assert.AreEqual(ResponseCode.Success, response.Code);
                Assert.NotNull(item);
                Assert.True(item.Id > 0);

                var assessment = this.db.SkillAssessment.First(a => a.Id == item.Id);

                Assert.NotNull(assessment);
                Assert.AreEqual(dt, assessment.ExamDate);
                Assert.Null(assessment.IsPassed);
                id = assessment.Id;
            }
            finally
            {
                if (id.HasValue)
                {
                    var sa = this.db.SkillAssessment.First(a => a.Id == id);
                    this.db.Remove(sa);
                    this.db.SaveChanges();
                }
            }
        }

        [TestCase]
        public async Task AssessmentPost_CreateUnknownTechnology()
        {
            var dt = new DateTime(2017, 12, 31, 14, 15, 0);
            var input = new AssessmentInputVm
            {
                TechnologyId = 100500,
                StudentId = this.expertB.Id,
                ExaminerId = this.expertC.Id,
                ExamDate = dt.Date,
                ExamTime = dt,
            };

            var request = this.Request(input, this.expertC);

            var response = await this.manager.InputPost(request);

            var item = response.Item;
            Assert.AreEqual(ResponseCode.ValidationError, response.Code);
            Assert.NotNull(item);
            Assert.Null(item.TechnologyName);

            Assert.False(request.ModelState.IsValid);
            Assert.AreEqual(1, request.ModelState.Count);
            Assert.True(request.ModelState[String.Empty].Errors.Any(e => e.ErrorMessage == ValidationMessages.Instance.UnknownTechnology));

            int count = this.db.SkillAssessment.Count();
            Assert.AreEqual(0, count);
        }

        [TestCase]
        public async Task AssessmentPost_CreateNonExpert()
        {
            var dt = new DateTime(2017, 12, 31, 14, 15, 0);
            var input = new AssessmentInputVm
            {
                TechnologyId = this.techA.Id,
                StudentId = this.expertB.Id,
                ExaminerId = this.expertC.Id,
                ExamDate = dt.Date,
                ExamTime = dt,
            };

            var request = this.Request(input, this.expertC);

            var response = await this.manager.InputPost(request);

            var item = response.Item;
            Assert.AreEqual(ResponseCode.ValidationError, response.Code);
            Assert.NotNull(item);
            Assert.True(this.IsAssessmentDataComplete(item));

            Assert.False(request.ModelState.IsValid);
            Assert.AreEqual(1, request.ModelState.Count);
            Assert.True(request.ModelState[String.Empty].Errors.Any(e => e.ErrorMessage == ValidationMessages.Instance.AssessmentExpertAssign(this.techA.Name)));

            int count = this.db.SkillAssessment.Count();
            Assert.AreEqual(0, count);
        }

        [TestCase]
        public async Task AssessmentPost_CreateAppointWrongExaminer()
        {
            var dt = new DateTime(2017, 12, 31, 14, 15, 0);
            var input = new AssessmentInputVm
            {
                TechnologyId = this.techA.Id,
                StudentId = this.expertB.Id,
                ExaminerId = this.expertC.Id,
                ExamDate = dt.Date,
                ExamTime = dt,
            };

            var request = this.Request(input, this.expertA);

            var response = await this.manager.InputPost(request);

            var item = response.Item;
            Assert.AreEqual(ResponseCode.ValidationError, response.Code);
            Assert.NotNull(item);
            Assert.True(this.IsAssessmentDataComplete(item));

            Assert.False(request.ModelState.IsValid);
            Assert.AreEqual(1, request.ModelState.Count);
            Assert.True(request.ModelState[String.Empty].Errors.Any(e => e.ErrorMessage == ValidationMessages.Instance.AssessmentCannotAppointExaminer));

            int count = this.db.SkillAssessment.Count();
            Assert.AreEqual(0, count);
        }

        [TestCase]
        public async Task AssessmentPost_CreateSelfAssessment()
        {
            var dt = new DateTime(2017, 12, 31, 14, 15, 0);
            var input = new AssessmentInputVm
            {
                TechnologyId = this.techA.Id,
                StudentId = this.expertA.Id,
                ExaminerId = this.expertA.Id,
                ExamDate = dt.Date,
                ExamTime = dt,
            };

            var request = this.Request(input, this.expertA);

            var response = await this.manager.InputPost(request);

            var item = response.Item;
            Assert.AreEqual(ResponseCode.ValidationError, response.Code);
            Assert.NotNull(item);
            Assert.True(this.IsAssessmentDataComplete(item));

            Assert.False(request.ModelState.IsValid);
            Assert.AreEqual(1, request.ModelState.Count);
            Assert.True(request.ModelState[String.Empty].Errors.Any(e => e.ErrorMessage == ValidationMessages.Instance.AssessmentSelf));

            int count = this.db.SkillAssessment.Count();
            Assert.AreEqual(0, count);
        }

        [TestCase]
        public async Task AssessmentPost_UpdateSuccess()
        {
            SkillAssessment existingAssessment;

            using (var storage = NewDbContext())
            {
                existingAssessment = new SkillAssessment
                {
                    TechnologyId = this.techA.Id,
                    StudentId = this.expertB.Id,
                    ExaminerId = this.expertA.Id,
                    ExamDate = new DateTime(2000, 1, 1),
                };
                storage.Add(existingAssessment);
                storage.SaveChanges();
            }

            var dt = new DateTime(2017, 12, 31, 14, 15, 0);
            var input = new AssessmentInputVm
            {
                Id = existingAssessment.Id,
                TechnologyId = existingAssessment.TechnologyId,
                StudentId = existingAssessment.StudentId,
                ExaminerId = existingAssessment.ExaminerId,
                ExamDate = dt.Date,
                ExamTime = dt,
            };

            try
            {
                var request = this.Request(input, this.expertA);

                var response = await this.manager.InputPost(request);

                var item = response.Item;
                Assert.AreEqual(ResponseCode.Success, response.Code);
                Assert.NotNull(item);
                Assert.AreEqual(existingAssessment.Id, item.Id);

                var assessment = this.db.SkillAssessment.Single(a => a.Id == existingAssessment.Id);

                Assert.AreEqual(dt, assessment.ExamDate);
                Assert.Null(assessment.IsPassed);
            }
            finally
            {
                var sa = this.db.SkillAssessment.Single(a => a.Id == existingAssessment.Id);
                this.db.Remove(sa);
                this.db.SaveChanges();

                int count = this.db.SkillAssessment.Count();
                Assert.AreEqual(0, count);
            }
        }

        [TestCase]
        public async Task AssessmentPost_UpdateWrongData()
        {
            SkillAssessment existingAssessment;

            using (var storage = NewDbContext())
            {
                existingAssessment = new SkillAssessment
                {
                    TechnologyId = this.techA.Id,
                    StudentId = this.expertB.Id,
                    ExaminerId = this.expertA.Id,
                    ExamDate = new DateTime(2000, 1, 1),
                };
                storage.Add(existingAssessment);
                storage.SaveChanges();
            }

            var dt = new DateTime(2017, 12, 31, 14, 15, 0);
            var input = new AssessmentInputVm
            {
                Id = existingAssessment.Id,
                TechnologyId = this.techC.Id,
                StudentId = existingAssessment.StudentId,
                ExaminerId = this.expertC.Id,
                ExamDate = dt.Date,
                ExamTime = dt,
            };

            try
            {
                var request = this.Request(input, this.expertC);

                var response = await this.manager.InputPost(request);

                var item = response.Item;
                Assert.AreEqual(ResponseCode.ValidationError, response.Code);
                Assert.NotNull(item);

                Assert.False(request.ModelState.IsValid);
                Assert.AreEqual(1, request.ModelState.Count);
                Assert.True(request.ModelState[String.Empty].Errors.Any(e => e.ErrorMessage == ValidationMessages.Instance.AssessmentInvalidDetails));

                Assert.AreEqual(existingAssessment.Id, item.Id);
                Assert.True(this.IsAssessmentDataComplete(item));

                var assessment = this.db.SkillAssessment.Single(a => a.Id == existingAssessment.Id);

                Assert.AreEqual(existingAssessment.ExamDate, assessment.ExamDate);
                Assert.Null(assessment.IsPassed);
            }
            finally
            {
                var sa = this.db.SkillAssessment.Single(a => a.Id == existingAssessment.Id);
                this.db.Remove(sa);
                this.db.SaveChanges();

                int count = this.db.SkillAssessment.Count();
                Assert.AreEqual(0, count);
            }
        }

        [TestCase]
        public async Task AssessmentPost_UpdatePassedAssessment()
        {
            SkillAssessment existingAssessment;

            using (var storage = NewDbContext())
            {
                existingAssessment = new SkillAssessment
                {
                    TechnologyId = this.techA.Id,
                    StudentId = this.expertB.Id,
                    ExaminerId = this.expertA.Id,
                    ExamDate = new DateTime(2000, 1, 1),
                    IsPassed = true,
                };
                storage.Add(existingAssessment);
                storage.SaveChanges();
            }

            var dt = new DateTime(2017, 12, 31, 14, 15, 0);
            var input = new AssessmentInputVm
            {
                Id = existingAssessment.Id,
                TechnologyId = existingAssessment.TechnologyId,
                StudentId = existingAssessment.StudentId,
                ExaminerId = existingAssessment.ExaminerId,
                ExamDate = dt.Date,
                ExamTime = dt,
            };

            try
            {
                var request = this.Request(input, this.expertA);

                var response = await this.manager.InputPost(request);

                var item = response.Item;
                Assert.AreEqual(ResponseCode.ValidationError, response.Code);
                Assert.NotNull(item);

                Assert.False(request.ModelState.IsValid);
                Assert.AreEqual(1, request.ModelState.Count);
                Assert.True(request.ModelState[String.Empty].Errors.Any(e => e.ErrorMessage == ValidationMessages.Instance.AssessmentPastEvent));

                Assert.AreEqual(existingAssessment.Id, item.Id);
                Assert.True(this.IsAssessmentDataComplete(item));

                var assessment = this.db.SkillAssessment.Single(a => a.Id == existingAssessment.Id);

                Assert.AreEqual(existingAssessment.ExamDate, assessment.ExamDate);
                Assert.AreEqual(existingAssessment.StudentId, assessment.StudentId);
                Assert.AreEqual(existingAssessment.ExaminerId, assessment.ExaminerId);
                Assert.AreEqual(existingAssessment.TechnologyId, assessment.TechnologyId);
                Assert.True(assessment.IsPassed);
            }
            finally
            {
                var sa = this.db.SkillAssessment.Single(a => a.Id == existingAssessment.Id);
                this.db.Remove(sa);
                this.db.SaveChanges();

                int count = this.db.SkillAssessment.Count();
                Assert.AreEqual(0, count);
            }
        }

        [TestCase]
        public async Task AssessmentDetails_WrongId()
        {
            var assessment = new SkillAssessment
            {
                TechnologyId = this.techA.Id,
                StudentId = this.expertB.Id,
                ExaminerId = this.expertA.Id,
                ExamDate = new DateTime(2000, 1, 1),
                IsPassed = true,
            };
            this.db.Add(assessment);
            this.db.SaveChanges();

            try
            {
                var request = this.Request(100500, this.expertA);

                var response = await this.manager.DetailsGet(request);

                Assert.AreEqual(ResponseCode.NotFound, response.Code);
                Assert.Null(response.Item);
            }
            finally
            {
                this.db.Remove(assessment);
                this.db.SaveChanges();

                int count = this.db.SkillAssessment.Count();
                Assert.AreEqual(0, count);
            }
        }

        [TestCase]
        public async Task AssessmentDetails_Examiner()
        {
            var existingAssessment = new SkillAssessment
            {
                TechnologyId = this.techA.Id,
                StudentId = this.expertB.Id,
                ExaminerId = this.expertA.Id,
                ExamDate = new DateTime(2000, 1, 1),
            };
            this.db.Add(existingAssessment);
            this.db.SaveChanges();

            try
            {
                var request = this.Request(existingAssessment.Id, this.expertA);

                var response = await this.manager.DetailsGet(request);

                var item = response.Item;
                Assert.AreEqual(ResponseCode.Success, response.Code);
                Assert.NotNull(item);
                Assert.AreEqual(existingAssessment.Id, item.Id);
                Assert.True(this.IsAssessmentDataComplete(item));
                Assert.True(item.CanEdit && item.CanMark && item.CanDelete);

                Assert.AreEqual(existingAssessment.ExamDate, item.ExamDate);
                Assert.AreEqual(existingAssessment.StudentId, item.Student.Id);
                Assert.AreEqual(existingAssessment.ExaminerId, item.Examiner.Id);
                Assert.AreEqual(existingAssessment.TechnologyId, item.Technology.Id);
            }
            finally
            {
                this.db.Remove(existingAssessment);
                this.db.SaveChanges();

                int count = this.db.SkillAssessment.Count();
                Assert.AreEqual(0, count);
            }
        }

        [TestCase]
        public async Task AssessmentDetails_NonExpert()
        {
            var existingAssessment = new SkillAssessment
            {
                TechnologyId = this.techA.Id,
                StudentId = this.expertB.Id,
                ExaminerId = this.expertA.Id,
                ExamDate = new DateTime(2000, 1, 1),
                IsPassed = true,
            };
            this.db.Add(existingAssessment);
            this.db.SaveChanges();

            try
            {
                var request = this.Request(existingAssessment.Id, this.expertC);

                var response = await this.manager.DetailsGet(request);

                var item = response.Item;
                Assert.AreEqual(ResponseCode.Success, response.Code);
                Assert.NotNull(item);
                Assert.AreEqual(existingAssessment.Id, item.Id);
                Assert.True(this.IsAssessmentDataComplete(item));
                Assert.False(item.CanEdit || item.CanMark || item.CanDelete);

                Assert.AreEqual(existingAssessment.ExamDate, item.ExamDate);
                Assert.AreEqual(existingAssessment.StudentId, item.Student.Id);
                Assert.AreEqual(existingAssessment.ExaminerId, item.Examiner.Id);
                Assert.AreEqual(existingAssessment.TechnologyId, item.Technology.Id);
            }
            finally
            {
                this.db.Remove(existingAssessment);
                this.db.SaveChanges();

                int count = this.db.SkillAssessment.Count();
                Assert.AreEqual(0, count);
            }
        }

        [TestCase]
        public async Task AssessmentMarkGet_Examiner()
        {
            var existingAssessment = new SkillAssessment
            {
                TechnologyId = this.techA.Id,
                StudentId = this.expertB.Id,
                ExaminerId = this.expertA.Id,
                ExamDate = new DateTime(2000, 1, 1),
            };
            this.db.Add(existingAssessment);
            this.db.SaveChanges();

            try
            {
                var request = this.Request(existingAssessment.Id, this.expertA);

                var response = await this.manager.MarkGet(request);

                var item = response.Item;
                Assert.AreEqual(ResponseCode.Success, response.Code);
                Assert.NotNull(item);
            }
            finally
            {
                this.db.Remove(existingAssessment);
                this.db.SaveChanges();

                int count = this.db.SkillAssessment.Count();
                Assert.AreEqual(0, count);
            }
        }

        [TestCase]
        public async Task AssessmentMarkGet_WrongId()
        {
            var request = this.Request(100, this.expertA);

            var response = await this.manager.MarkGet(request);

            Assert.AreEqual(ResponseCode.NotFound, response.Code);
            Assert.Null(response.Item);
        }

        [TestCase]
        public async Task AssessmentMarkGet_ExaminerIsPassed()
        {
            var existingAssessment = new SkillAssessment
            {
                TechnologyId = this.techA.Id,
                StudentId = this.expertB.Id,
                ExaminerId = this.expertA.Id,
                ExamDate = new DateTime(2000, 1, 1),
                IsPassed = true,
            };
            this.db.Add(existingAssessment);
            this.db.SaveChanges();

            try
            {
                var request = this.Request(existingAssessment.Id, this.expertA);

                var response = await this.manager.MarkGet(request);

                var item = response.Item;
                Assert.AreEqual(ResponseCode.NotFound, response.Code);
                Assert.Null(item);
            }
            finally
            {
                this.db.Remove(existingAssessment);
                this.db.SaveChanges();

                int count = this.db.SkillAssessment.Count();
                Assert.AreEqual(0, count);
            }
        }

        [TestCase]
        public async Task AssessmentMarkGet_NotExaminer()
        {
            var existingAssessment = new SkillAssessment
            {
                TechnologyId = this.techA.Id,
                StudentId = this.expertB.Id,
                ExaminerId = this.expertA.Id,
                ExamDate = new DateTime(2000, 1, 1),
            };
            this.db.Add(existingAssessment);
            this.db.SaveChanges();

            try
            {
                var request = this.Request(existingAssessment.Id, this.expertB);

                var response = await this.manager.MarkGet(request);

                var item = response.Item;
                Assert.AreEqual(ResponseCode.NotFound, response.Code);
                Assert.Null(item);
            }
            finally
            {
                this.db.Remove(existingAssessment);
                this.db.SaveChanges();

                int count = this.db.SkillAssessment.Count();
                Assert.AreEqual(0, count);
            }
        }

        [TestCase]
        public async Task AssessmentMarkPost_NotExaminer()
        {
            var existingAssessment = new SkillAssessment
            {
                TechnologyId = this.techA.Id,
                StudentId = this.expertB.Id,
                ExaminerId = this.expertA.Id,
                ExamDate = new DateTime(2000, 1, 1),
            };
            this.db.Add(existingAssessment);
            this.db.SaveChanges();

            var mark = new AssessmentDetailsVm
            {
                Id = existingAssessment.Id,
                ExaminerId = this.expertB.Id,
                IsPassed = true,
            };

            try
            {
                var request = this.Request(mark, this.expertB);

                var response = await this.manager.MarkPost(request);

                var item = response.Item;
                Assert.AreEqual(ResponseCode.ValidationError, response.Code);
                Assert.NotNull(item);
                Assert.True(IsAssessmentDataComplete(item));

                Assert.False(request.ModelState.IsValid);
                Assert.AreEqual(1, request.ModelState.Count);
                Assert.True(request.ModelState[String.Empty].Errors.Any(e => e.ErrorMessage == ValidationMessages.Instance.AssessmentNonExaminerMark));
            }
            finally
            {
                this.db.Remove(existingAssessment);
                this.db.SaveChanges();

                int count = this.db.SkillAssessment.Count();
                Assert.AreEqual(0, count);
            }
        }

        [TestCase]
        public async Task AssessmentMarkPost_ExaminerMarkPassed()
        {
            SkillAssessment existingAssessment;
            using (var storage = NewDbContext())
            {
                existingAssessment = new SkillAssessment
                {
                    TechnologyId = this.techA.Id,
                    StudentId = this.expertB.Id,
                    ExaminerId = this.expertA.Id,
                    ExamDate = new DateTime(2000, 1, 1),
                };
                storage.Add(existingAssessment);
                storage.SaveChanges();
            }

            var mark = new AssessmentDetailsVm
            {
                Id = existingAssessment.Id,
                ExaminerId = existingAssessment.ExaminerId,
                ExamComment = "hello",
                ExamScore = 99,
                IsPassed = true,
            };

            try
            {
                var request = this.Request(mark, this.expertA);

                var response = await this.manager.MarkPost(request);

                var item = response.Item;
                Assert.AreEqual(ResponseCode.Success, response.Code);
                Assert.NotNull(item);

                var assessment = this.db.SkillAssessment.Single(a => a.Id == existingAssessment.Id);
                Assert.True(assessment.IsPassed);
            }
            finally
            {
                var sa = this.db.SkillAssessment.Single(a => a.Id == existingAssessment.Id);

                this.db.Remove(sa);
                this.db.SaveChanges();

                int count = this.db.SkillAssessment.Count();
                Assert.AreEqual(0, count);
            }
        }

        [TestCase]
        public async Task AssessmentMarkPost_ExaminerMarkFailed()
        {
            SkillAssessment existingAssessment;
            using (var storage = NewDbContext())
            {
                existingAssessment = new SkillAssessment
                {
                    TechnologyId = this.techA.Id,
                    StudentId = this.expertB.Id,
                    ExaminerId = this.expertA.Id,
                    ExamDate = new DateTime(2000, 1, 1),
                };
                storage.Add(existingAssessment);
                storage.SaveChanges();
            }

            var mark = new AssessmentDetailsVm
            {
                Id = existingAssessment.Id,
                ExaminerId = existingAssessment.ExaminerId,
                ExamComment = "hello",
                ExamScore = 99,
                IsPassed = false,
            };

            try
            {
                var request = this.Request(mark, this.expertA);

                var response = await this.manager.MarkPost(request);

                var item = response.Item;
                Assert.AreEqual(ResponseCode.Success, response.Code);
                Assert.NotNull(item);

                var assessment = this.db.SkillAssessment.Single(a => a.Id == existingAssessment.Id);
                Assert.False(assessment.IsPassed);
            }
            finally
            {
                var sa = this.db.SkillAssessment.Single(a => a.Id == existingAssessment.Id);

                this.db.Remove(sa);
                this.db.SaveChanges();

                int count = this.db.SkillAssessment.Count();
                Assert.AreEqual(0, count);
            }
        }

        [TestCase]
        public async Task AssessmentMarkPost_ExaminerMarkUnsuccessful()
        {
            SkillAssessment existingAssessment;
            using (var storage = NewDbContext())
            {
                existingAssessment = new SkillAssessment
                {
                    TechnologyId = this.techA.Id,
                    StudentId = this.expertB.Id,
                    ExaminerId = this.expertA.Id,
                    ExamDate = new DateTime(2000, 1, 1),
                    IsPassed = false,
                };
                storage.Add(existingAssessment);
                storage.SaveChanges();
            }

            var mark = new AssessmentDetailsVm
            {
                Id = existingAssessment.Id,
                ExaminerId = existingAssessment.ExaminerId,
                ExamComment = "hello",
                ExamScore = 99,
                IsPassed = true,
            };

            try
            {
                var request = this.Request(mark, this.expertA);

                var response = await this.manager.MarkPost(request);

                var item = response.Item;
                Assert.AreEqual(ResponseCode.ValidationError, response.Code);
                Assert.NotNull(item);

                Assert.False(request.ModelState.IsValid);
                Assert.AreEqual(1, request.ModelState.Count);
                Assert.True(request.ModelState[String.Empty].Errors.Any(e => e.ErrorMessage == ValidationMessages.Instance.AssessmentMarked));

                var assessment = this.db.SkillAssessment.Single(a => a.Id == existingAssessment.Id);
                Assert.False(assessment.IsPassed);
            }
            finally
            {
                var sa = this.db.SkillAssessment.Single(a => a.Id == existingAssessment.Id);

                this.db.Remove(sa);
                this.db.SaveChanges();

                int count = this.db.SkillAssessment.Count();
                Assert.AreEqual(0, count);
            }
        }

        [TestCase]
        public async Task AssessmentMarkPost_ExaminerMarkWrongId()
        {
            SkillAssessment existingAssessment;
            using (var storage = NewDbContext())
            {
                existingAssessment = new SkillAssessment
                {
                    TechnologyId = this.techA.Id,
                    StudentId = this.expertB.Id,
                    ExaminerId = this.expertA.Id,
                    ExamDate = new DateTime(2000, 1, 1),
                };
                storage.Add(existingAssessment);
                storage.SaveChanges();
            }

            var mark = new AssessmentDetailsVm
            {
                Id = 100500,
                ExaminerId = existingAssessment.ExaminerId,
                ExamComment = "hello",
                ExamScore = 99,
                IsPassed = true,
            };

            try
            {
                var request = this.Request(mark, this.expertA);

                var response = await this.manager.MarkPost(request);

                var item = response.Item;
                Assert.AreEqual(ResponseCode.NotFound, response.Code);
                Assert.Null(item);

                var assessment = this.db.SkillAssessment.Single(a => a.Id == existingAssessment.Id);
                Assert.Null(assessment.IsPassed);
            }
            finally
            {
                var sa = this.db.SkillAssessment.Single(a => a.Id == existingAssessment.Id);

                this.db.Remove(sa);
                this.db.SaveChanges();

                int count = this.db.SkillAssessment.Count();
                Assert.AreEqual(0, count);
            }
        }

        [TestCase]
        public async Task AssessmentDelete_Examiner()
        {
            SkillAssessment existingAssessment;
            using (var storage = NewDbContext())
            {
                existingAssessment = new SkillAssessment
                {
                    TechnologyId = this.techA.Id,
                    StudentId = this.expertB.Id,
                    ExaminerId = this.expertA.Id,
                    ExamDate = new DateTime(2000, 1, 1),
                };
                storage.Add(existingAssessment);
                storage.SaveChanges();
            }

            var request = this.Request(existingAssessment.Id, this.expertA);

            var response = await this.manager.DeleteGet(request);

            Assert.AreEqual(ResponseCode.Success, response.Code);
            Assert.NotNull(response.Item);
            Assert.True(this.IsAssessmentDataComplete(response.Item));

            int count = this.db.SkillAssessment.Count(a => a.Id == existingAssessment.Id);
            Assert.AreEqual(1, count);

            response = await this.manager.DeletePost(request);

            Assert.AreEqual(ResponseCode.Success, response.Code);

            count = this.db.SkillAssessment.Count(a => a.Id == existingAssessment.Id);
            Assert.AreEqual(0, count);
        }

        [TestCase]
        public async Task AssessmentDelete_WrongId()
        {
            var request = this.Request(100, this.expertA);

            var response = await this.manager.DeleteGet(request);

            Assert.AreEqual(ResponseCode.NotFound, response.Code);
            Assert.Null(response.Item);

            response = await this.manager.DeletePost(request);

            Assert.AreEqual(ResponseCode.NotFound, response.Code);
            Assert.Null(response.Item);
        }

        [TestCase]
        public async Task AssessmentDelete_NonExaminer()
        {
            SkillAssessment existingAssessment;
            using (var storage = NewDbContext())
            {
                existingAssessment = new SkillAssessment
                {
                    TechnologyId = this.techA.Id,
                    StudentId = this.expertB.Id,
                    ExaminerId = this.expertA.Id,
                    ExamDate = new DateTime(2000, 1, 1),
                };
                storage.Add(existingAssessment);
                storage.SaveChanges();
            }

            try
            {
                var request = this.Request(existingAssessment.Id, this.expertB);

                var response = await this.manager.DeleteGet(request);

                Assert.AreEqual(ResponseCode.NotFound, response.Code);
                Assert.Null(response.Item);

                int count = this.db.SkillAssessment.Count(a => a.Id == existingAssessment.Id);
                Assert.AreEqual(1, count);

                response = await this.manager.DeletePost(request);

                Assert.AreEqual(ResponseCode.ValidationError, response.Code);

                Assert.False(request.ModelState.IsValid);
                Assert.AreEqual(1, request.ModelState.Count);
                Assert.True(request.ModelState[String.Empty].Errors.Any(e => e.ErrorMessage == ValidationMessages.Instance.AssessmentCannotDelete));

                count = this.db.SkillAssessment.Count(a => a.Id == existingAssessment.Id);
                Assert.AreEqual(1, count);
            }
            finally
            {
                var sa = this.db.SkillAssessment.Single(a => a.Id == existingAssessment.Id);
                this.db.Remove(sa);
                this.db.SaveChanges();
            }
        }
    }
}
