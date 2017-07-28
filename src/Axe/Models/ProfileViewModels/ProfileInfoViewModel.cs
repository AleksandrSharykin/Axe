using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models.ProfileViewModels
{
    public class ProfileInfoViewModel: IndexViewModel
    {
        public IEnumerable<Technology> Technologies { get; set; }

        public Technology SelectedTechnology { get; set; }

        public IEnumerable <ExamTask> Tasks { get; set; }

        public IEnumerable<ExamAttempt> AllAttempts { get; set; }

        public IEnumerable<ExamAttempt> BestAttempts { get; set; }
    }
}
