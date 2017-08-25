using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Axe.Models;

namespace Axe.Managers
{
    public class StatisticsManager : ManagerBase, IStatisticsManager
    {
        public StatisticsManager(AxeDbContext context) : base(context)
        {
        }

        public int MembersCount
        {
            get { return this.context.Users.Count(); }
        }

        public async Task<IList<object>> GetExams(DateTime periodStart, DateTime periodEnd)
        {
            periodStart = periodStart.Date;
            periodEnd = periodEnd.Date.AddDays(1);

            var exams = await this.context.ExamAttempt
                .Where(a => a.ExamDate >= periodStart && a.ExamDate <= periodEnd)
                .GroupBy(a => a.ExamDate.Value.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { Day = g.Key.ToString("dd.MM.yyyy"), Count = g.Count() })
                .ToListAsync<object>();
            return exams;
        }
    }
}
