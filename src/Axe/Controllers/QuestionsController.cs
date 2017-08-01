using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Axe.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Axe.Controllers
{
    public class QuestionsController : Controller
    {
        public QuestionsController()
        {

        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create(int? technologyId = null)
        {
            var data = Enumerable.Range(1, 3).Select(i => GetAnswer(i)).ToList();
            var q = new TaskQuestion()
            {
                Text = "ask me something",
                Answers = data,
            };
            return View(q);
        }

        private TaskAnswer GetAnswer(int num)
        {
            return new TaskAnswer() { Id = num, Text = "Something complex tl;dr (Ans #" + num + ")", Value = "true" };
        }

        [HttpPost]
        public IActionResult Create(TaskQuestion questionVm, string cmd = null)
        {
            // https://stackoverflow.com/questions/37490192/modelbinding-on-model-collection
            if (cmd?.Trim()?.ToLower() == "add")
                questionVm.Answers.Add(GetAnswer(questionVm.Answers.Count + 1));
            return View(questionVm);
        }


        public IActionResult Edit(int? id)
        {
            return View(new TaskQuestion());
        }
    }
}
