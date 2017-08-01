using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Axe.Models.QuestionVm
{
    public class QuestionInputVm
    {
        public SelectList Technologies { get; set; }

        public int TechnologyId { get; set; }

        public int QuestionId { get; set; }

        public string QuestionText { get; set; }

        public IList<SelectableTaskAnswer> Answers { get; set; }
    }
}
