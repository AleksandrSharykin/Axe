using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Axe.Models.ProfileViewModels
{
    public class IndexViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }

        public string ContactInfo { get; set; }

        public bool HasPassword { get; set; }

        public IList<UserLoginInfo> Logins { get; set; }

        public string PhoneNumber { get; set; }

        public bool TwoFactor { get; set; }

        public bool BrowserRemembered { get; set; }

        public IList<UserTechnologyStats> TechStats { get; set; } = new List<UserTechnologyStats>();
    }
}
