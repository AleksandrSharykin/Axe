using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Managers
{
    public interface IStatisticsManager
    {
        int MembersCount { get; }

        Task<IList<object>> GetExams(DateTime periodStart, DateTime periodEnd);

        Task<IList<object>> GetQuestionsDifficulty();

        Task<IList<object>> GetTechnologiesDifficulty();
    }
}
