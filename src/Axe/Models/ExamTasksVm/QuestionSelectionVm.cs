using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Models.ExamTasksVm
{
    public class QuestionSelectionVm
    {
        public int Id { get; set; }

        public string Text { get; set; }

        public string Preview
        {
            get
            {
                return Text != null && Text.Length > 128 ? Text.Substring(0, 128) + "..." : Text;
            }
        }

        public bool IsSelected { get; set; }
    }
}
