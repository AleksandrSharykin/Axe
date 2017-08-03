using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    public class ExpertTechnologyLink
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int TechnologyId { get; set; }
        public Technology Technology { get; set; }
    }
}
