using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Axe.Dto;
using Axe.Managers;

namespace Axe.Controllers
{
    /// <summary>
    /// Controller provides statistic values for statistics ajax requests
    /// </summary>
    [Produces("application/json")]
    [Route("api/statistics/[action]")]
    public class StatisticsApiController : Controller
    {
        private IStatisticsManager manager;
        public StatisticsApiController(IStatisticsManager manager)
        {
            this.manager = manager;
        }

        /// <summary>
        /// Gets the number of registered users
        /// </summary>
        [HttpGet]
        public int GetMembersCount()
        {
            return this.manager.MembersCount;
        }

        /// <summary>
        /// Gets a list of exam attempts in a certain date range
        /// </summary>
        /// <param name="periodStart">First day of a period</param>
        /// <param name="periodEnd">Last day of a period</param>
        [HttpGet]
        public async Task<IList<StatsExamAttempts>> GetExams(DateTime periodStart, DateTime periodEnd)
        {
            return await this.manager.GetExams(periodStart, periodEnd);
        }

        /// <summary>
        /// Gets a list of examiners with their skill assessment in each technology
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IList<StatsExaminer>> GetExaminers()
        {
            return await this.manager.GetExaminers();
        }

        /// <summary>
        /// Gets a list of technologies with average exam scores
        /// </summary>
        [HttpGet]
        public async Task<IList<StatsTechnology>> GetTechnologiesDifficulty()
        {
            return await this.manager.GetTechnologiesDifficulty();
        }

        /// <summary>
        /// Gets a list of question with their success rating 
        /// </summary>
        [HttpGet]
        public async Task<IList<StatsQuestion>> GetQuestionsDifficulty()
        {
            return await this.manager.GetQuestionsDifficulty();
        }
    }
}