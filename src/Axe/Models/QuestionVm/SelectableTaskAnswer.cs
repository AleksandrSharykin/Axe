using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models.QuestionVm
{
    public class SelectableTaskAnswer : TaskAnswer
    {        
        public bool IsChecked
        {
            get { return bool.Parse(Value); }
            set
            {
                Value = value.ToString();
            }
        }
    }
}
