using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    /// <summary>
    /// Response status codes
    /// </summary>
    public enum ResponseCode
    {
        /// <summary>
        /// Manager successfully completed requested operation 
        /// </summary>
        Success = 0,
        /// <summary>
        /// Request contains invalid data 
        /// </summary>
        ValidationError = 1,
        /// <summary>
        /// Requested 
        /// </summary>
        NotFound = 404,
    }
}
