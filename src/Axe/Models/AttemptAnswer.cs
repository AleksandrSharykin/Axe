using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

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

        [NotMapped]
        public bool IsSelected
        {
            get { return bool.Parse(Value ?? Boolean.FalseString); }
            set { Value = value.ToString(); }
        }
    }
}
