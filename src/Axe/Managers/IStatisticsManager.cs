using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Axe.Dto;

namespace Axe.Managers
{
    /// <summary>
    /// Interface declares operations governed by <see cref="Axe.Controllers.StatisticsApiController"/>
    /// </summary>
    public interface IStatisticsManager
    {
        /// <summary>
        /// Gets the number of registered users
        /// </summary>
        int MembersCount { get; }

        /// <summary>
        /// Gets a list of exam attempts in a certain date range
        /// </summary>
        /// <param name="periodStart">First day of a period</param>
        /// <param name="periodEnd">Last day of a period</param>
        /// <returns></returns>
        Task<IList<StatsExamAttempts>> GetExams(DateTime periodStart, DateTime periodEnd);

        /// <summary>
        /// Gets a list of examiners with their skill assessment in each technology
        /// </summary>
        /// <returns></returns>
        Task<IList<StatsExaminer>> GetExaminers();

        /// <summary>
        /// Gets a list of technologies with average exam scores
        /// </summary>
        /// <returns></returns>
        Task<IList<StatsTechnology>> GetTechnologiesDifficulty();

        /// <summary>
        /// Gets a list of question with their success rating 
        /// </summary>
        /// <returns></returns>
        Task<IList<StatsQuestion>> GetQuestionsDifficulty();
    }
}
