using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Axe.Dto;
using Axe.Models;

namespace Axe.Managers
{
    /// <summary>
    /// Class implements operations governed by <see cref="Axe.Controllers.StatisticsApiController"/>
    /// </summary>
    public class StatisticsManager : ManagerBase, IStatisticsManager
    {
        public StatisticsManager(AxeDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets the number of registered users
        /// </summary>
        public int MembersCount
        {
            get { return this.context.Users.Count(); }
        }

        /// <summary>
        /// Gets a list of exam attempts in a certain date range
        /// </summary>
        /// <param name="periodStart">First day of a period</param>
        /// <param name="periodEnd">Last day of a period</param>
        public async Task<IList<StatsExamAttempts>> GetExams(DateTime periodStart, DateTime periodEnd)
        {
            periodStart = periodStart.Date;
            periodEnd = periodEnd.Date.AddDays(1);

            var exams = await this.context.ExamAttempt
                .Where(a => a.ExamDate >= periodStart && a.ExamDate <= periodEnd)
                .GroupBy(a => a.ExamDate.Value.Date)
                .OrderBy(g => g.Key)
                .Select(g => new StatsExamAttempts
                {
                    Day = g.Key.ToString("dd.MM.yyyy"),
                    Count = g.Count()
                })
                .ToListAsync();
            return exams;
        }

        /// <summary>
        /// Gets a list of examiners with their skill assessment in each technology
        /// </summary>
        public async Task<IList<StatsExaminer>> GetExaminers()
        {
            IList<StatsExaminer> items = null;

            items = await this.context.Users.Include(u => u.AssessmentsAsExaminer).ThenInclude(a => a.Technology)
                .Where(u => u.AssessmentsAsExaminer.Any(a => a.IsPassed != null))
                .SelectMany(u => u.AssessmentsAsExaminer.GroupBy(a => a.Technology.Name),
                (u, gr) => new StatsExaminer
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Tech = gr.Key,
                    Successful = gr.Count(a => a.IsPassed == true),
                    Failed = gr.Count(a => a.IsPassed == false)
                })
                .ToListAsync();

            return items;
        }

        /// <summary>
        /// Gets a list of technologies with average exam scores
        /// </summary>
        public async Task<IList<StatsTechnology>> GetTechnologiesDifficulty()
        {
            var items = await this.context.Technology.Include(t => t.Assessments).Include(t => t.Attempts).ToListAsync();
            return items.Select(t => new StatsTechnology
            {
                Id = t.Id,
                Name = t.Name,
                AttemptsCount = t.Attempts.Where(a => a.IsFinished).Count(),
                AvgAttemptScore = t.Attempts.Where(a => a.IsFinished).Average(a => a.ExamScore * 100 / a.MaxScore)?.ToString("N2") ?? string.Empty,

                AssessmentsCount = t.Assessments.Count,
                AvgAssessmentScore = t.Assessments.Where(a => a.IsPassed == true).Average(a => a.ExamScore)?.ToString("N2") ?? string.Empty,
            })
            .ToList();
        }

        /// <summary>
        /// Gets a list of question with their success rating 
        /// </summary>
        public async Task<IList<StatsQuestion>> GetQuestionsDifficulty()
        {
            var items = await this.context.AttemptQuestion.Include(q => q.TaskQuestion).ThenInclude(q => q.Technology)
                .Where(q => q.Attempt.IsFinished)
                .GroupBy(q => q.TaskQuestion)
                .Select(g => new
                {
                    g.Key,
                    Total = g.Count(),
                    Successful = g.Where(q => q.IsPerfect == true).Count(),
                })
                .ToAsyncEnumerable()
                .Select(g => new StatsQuestion
                {
                    Id = g.Key.Id,
                    TechnologyName = g.Key.Technology.Name,
                    Percentage = g.Successful * 100 / g.Total,
                    Successful = g.Successful,
                    Total = g.Total,
                    Preview = g.Key.Preview,
                })
                .OrderBy(q => q.Successful * 100.0 / q.Total)
                .ToList();

            return items;
        }
    }
}
