using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Axe.Models;
using Axe.Managers;

namespace Axe.Controllers
{
    [Produces("application/json")]
    [Route("api/statistics/[action]")]
    public class StatisticsApiController : Controller
    {
        private IStatisticsManager manager;
        public StatisticsApiController(IStatisticsManager manager)
        {
            this.manager = manager;
        }

        [HttpGet]
        public int GetMembersCount()
        {
            return this.manager.MembersCount;
        }

        public async Task<IList<object>> GetExams(DateTime periodStart, DateTime periodEnd)
        {
            return await this.manager.GetExams(periodStart, periodEnd);
        }

        public async Task<IList<object>> GetComplexQuestions()
        {
            return await this.manager.GetComplexQuestions();
        }
    }
}