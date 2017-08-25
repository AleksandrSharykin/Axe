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

        public IList<string> GetExams(DateTime periodStart, DateTime periodEnd)
        {
            var days = (periodEnd - periodStart).Days;
            return Enumerable.Range(1, days).Select(d => String.Format("{0} - {1}", periodStart.AddDays(d).ToString("dd.MM.yyyy"), d)).ToList();
        }
    }
}