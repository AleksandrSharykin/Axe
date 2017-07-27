using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models
{
    public class AttemptAnswer
    {
        public int Id { get; set; }

        public int AttemptQuestionId { get; set; }

        public AttemptQuestion AttemptQuestion { get; set;}
        
        public int? TaskAnswerId { get; set; }

        public TaskAnswer TaskAnswer { get; set; }

        public string Value { get; set; }
    }
}
