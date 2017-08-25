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

        public async Task<IList<object>> GetComplexQuestions()
        {
            var items = await this.context.AttemptQuestion.Include(q => q.TaskQuestion).ThenInclude(q => q.Technology)
                .Where(q => q.Attempt.IsFinished)
                .GroupBy(q => q.TaskQuestion)
                .Select(g => new
                {
                    Key = g.Key,
                    Total = g.Count(),
                    Successful = g.Where(q => q.IsAccepted == true).Count(),
                })
                .ToAsyncEnumerable()
                .Select(g => new
                {
                    Id = g.Key.Id,
                    TechnologyName = g.Key.Technology.Name,
                    Percentage = g.Successful * 100 / g.Total,
                    g.Successful,
                    g.Total,
                    g.Key.Preview,
                })
                .OrderBy(q => q.Successful * 100.0 / q.Total)
                .ToList<object>();

            return items;
        }
    }
}
