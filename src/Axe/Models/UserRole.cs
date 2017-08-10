using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    /// <summary>
    /// Class contains information about existing user roles
    /// </summary>
    public static class UserRole
    {
        /// <summary>
        /// Gets administrator role name
        /// </summary>
        public static readonly string Superuser = "superuser";

        /// <summary>
        /// Gets registered user role name
        /// </summary>
        public static readonly string Member = "member";        
    }
}
