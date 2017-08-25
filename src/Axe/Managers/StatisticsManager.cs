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

        public async Task<IList<object>> GetExaminers()
        {
            IList<object> items = null;

            items = await this.context.Users.Include(u => u.AssessmentsAsExaminer).ThenInclude(a => a.Technology)
                .Where(u => u.AssessmentsAsExaminer.Any(a => a.IsPassed != null))
                .SelectMany(u => u.AssessmentsAsExaminer.GroupBy(a => a.Technology.Name),
                (u, gr) => new
                {
                    u.Id,
                    u.UserName,
                    Tech = gr.Key,
                    Successful = gr.Count(a => a.IsPassed == true),
                    Failed = gr.Count(a => a.IsPassed == false)
                })
                .ToListAsync<object>();

            return items;
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

        public async Task<IList<object>> GetQuestionsDifficulty()
        {
            var items = await this.context.AttemptQuestion.Include(q => q.TaskQuestion).ThenInclude(q => q.Technology)
                .Where(q => q.Attempt.IsFinished)
                .GroupBy(q => q.TaskQuestion)
                .Select(g => new
                {
                    g.Key,
                    Total = g.Count(),
                    Successful = g.Where(q => q.IsAccepted == true).Count(),
                })
                .ToAsyncEnumerable()
                .Select(g => new
                {
                    g.Key.Id,
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

        public async Task<IList<object>> GetTechnologiesDifficulty()
        {
            var items = await this.context.Technology.Include(t => t.Assessments).Include(t => t.Attempts).ToListAsync();
            return items.Select(t => new
            {
                t.Id,
                t.Name,
                AttemptsCount = t.Attempts.Where(a => a.IsFinished).Count(),
                AvgAttemptScore = t.Attempts.Where(a => a.IsFinished).Average(a => a.ExamScore * 100 / a.MaxScore)?.ToString("N2") ?? string.Empty,

                AssessmentsCount = t.Assessments.Count,
                AvgAssessmentScore = t.Assessments.Where(a => a.IsPassed == true).Average(a => a.ExamScore)?.ToString("N2") ?? string.Empty,
            })
            .ToList<object>();
        }
    }
}
