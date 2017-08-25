using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    }
}
