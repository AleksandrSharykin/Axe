using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Axe.Models.ProfileViewModels
{
    public class IndexVm
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string JobPosition { get; set; }

        public string ContactInfo { get; set; }        

        public IList<SkillAssessment> Skills { get; set; } = new List<SkillAssessment>();
    }
}
