using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Axe.Models
{
    /// <summary>
    /// Class contains answer value provided by user
    /// </summary>
    public class AttemptAnswer
    {
        /// <summary>
        /// Gets or sets identifier
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Get or sets associated question
        /// </summary>
        public int AttemptQuestionId { get; set; }

        /// <summary>
        /// Get or sets associated question
        /// </summary>
        public AttemptQuestion AttemptQuestion { get; set; }


        /// <summary>
        /// Gets or sets base <see cref="TaskAnswer"/>
        /// </summary>
        public int? TaskAnswerId { get; set; }

        /// <summary>
        /// Gets or sets base <see cref="TaskAnswer"/>
        /// </summary>
        public TaskAnswer TaskAnswer { get; set; }


        /// <summary>
        /// Gets or sets answer value provided by user
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets Value in cases when it should have two states (true/false)
        /// </summary>
        [NotMapped]
        public bool IsSelected
        {
            get
            {
                bool b;
                return bool.TryParse(Value, out b) && b;
            }
            set { Value = value.ToString(); }
        }

        /// <summary>
        /// Gets or sets answer position in question answers list
        /// </summary>
        public int SortNumber { get; set; }
    }
}
