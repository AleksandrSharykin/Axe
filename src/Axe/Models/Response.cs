using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    /// <summary>
    /// Container class to pass data from Manager to Controller 
    /// </summary>
    public class Response<T>
    {        
        /// <summary>
        /// Gets or sets response data
        /// </summary>
        public T Item { get; set; }

        /// <summary>
        /// Gets or sets respose status code
        /// </summary>
        public ResponseCode Code { get; set; }

        public Response() {}

        public Response(T item)
        {
            this.Item = item;
        }
    }
}
