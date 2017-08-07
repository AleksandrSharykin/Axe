using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Axe.Models
{
    /// <summary>
    /// Container class to pass data from Controller to associated Manager
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Request<T>
    {        
        /// <summary>
        /// Gets or sets request data
        /// </summary>
        public T Item { get; set; }

        /// <summary>
        /// Gets or sets active user
        /// </summary>
        public ApplicationUser CurrentUser { get; set; }

        /// <summary>
        /// Gets or sets request model state
        /// </summary>
        public ModelStateDictionary ModelState { get; set; }

        public Request() { }

        public Request(T item)
        {
            this.Item = item;            
        }
    }
}
