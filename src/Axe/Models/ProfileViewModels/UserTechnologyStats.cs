using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models.ProfileViewModels
{
    public class UserTechnologyStats
    {
        public Technology Tech { get; set; }

        public DateTime? ExamDate { get; set; }

        public int? ExamScore { get; set; }
    }
}
