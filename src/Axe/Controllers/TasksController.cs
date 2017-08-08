using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Axe.Models;
using Axe.Models.ExamTasksVm;
using Microsoft.AspNetCore.Authorization;

namespace Axe.Controllers
{
    [Authorize]
    public class TasksController : ControllerExt
    {
        public TasksController(UserManager<ApplicationUser> userManager, AxeDbContext context) : base(userManager, context) { }

        // GET: ExamTasks
        public async Task<IActionResult> Index()
        {
            var axeDbContext = context.ExamTask.Include(e => e.Technology);
            return View(await axeDbContext.ToListAsync());
        }

        // GET: ExamTasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var examTask = await context.ExamTask
                .Include(e => e.Technology)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (examTask == null)
            {
                return NotFound();
            }

            return View(examTask);
        }

        // GET: ExamTasks/Edit/5
        public async Task<IActionResult> Input(int? id, int? technologyId = null)
        {
            ExamTask examTask = null;            

            if (id.HasValue)
            {
                // edit existing exam task
                examTask = await context.ExamTask.Include(t => t.Technology)
                                                 .Include(t => t.Questions)                                                 
                                                 .SingleOrDefaultAsync(m => m.Id == id);
                if (examTask == null)
                {
                    return NotFound();
                }
            }
            else
            {
                // create new exam task
                var tech = await this.context.Technology.SingleOrDefaultAsync(t => t.Id == technologyId);
                if (tech == null)
                {
                    return NotFound();
                }

                examTask = new ExamTask()
                {
                    Technology = tech,
                    Questions = new List<TaskQuestionLink>(),
                };
            }

            // get all questions for selected technology. for exam task 
            var questions = this.context.TaskQuestion.Where(q => q.TechnologyId == examTask.Technology.Id).ToList();

            var task = new TaskInputVm
            {
                Id = examTask.Id,
                TechnologyName = examTask.Technology.Name,
                TechnologyId = examTask.Technology.Id,
                Title = examTask.Title,
                Objective = examTask.Objective,    
                IsDemonstration = examTask.IsDemonstration,
                Questions = questions.Select(q => new QuestionSelectionVm
                {
                    Id = q.Id,
                    Text = q.Text,
                    Preview = q.Preview,
                    Type = q.Type,
                    IsSelected = examTask.Questions.Any(x => x.QuestionId == q.Id),                    
                }).ToList()
            };

            return View(task);
        }

        // POST: ExamTasks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Input(int id, TaskInputVm taskInput)
        {
            if (id != taskInput.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ExamTask task;

                    // questions selected for task
                    var ids = taskInput.Questions
                                        .Where(q => q.IsSelected)
                                        .Select(q => q.Id)
                                        .ToList();

                    if (taskInput.Id > 0)
                    {
                        task = await this.context.ExamTask.Include(t=>t.Questions).SingleOrDefaultAsync(t => t.Id == id);
                        task.Title = taskInput.Title;
                        task.Objective = taskInput.Objective;
                        task.IsDemonstration = taskInput.IsDemonstration;

                        // remove unselected questions
                        var deletedQuestion = task.Questions.Where(q => false == ids.Contains(q.QuestionId)).ToList();
                        foreach (var question in deletedQuestion)
                            task.Questions.Remove(question);
                        
                        // add new selected question 
                        foreach(int questionId in ids)
                        {
                            if (false == task.Questions.Any(q => q.QuestionId == questionId))
                            {
                                task.Questions.Add(new TaskQuestionLink { Task = task, QuestionId = questionId });
                            }
                        }

                        context.Update(task);
                    }
                    else
                    {
                        task = new ExamTask()
                        {
                            Title = taskInput.Title,
                            Objective = taskInput.Objective,
                            TechnologyId = taskInput.TechnologyId,        
                            IsDemonstration = taskInput.IsDemonstration
                        };

                        var taskQuestions = this.context.TaskQuestion.Where(q => ids.Contains(q.Id)).ToList();

                        task.Questions = taskQuestions.Select(q => new TaskQuestionLink { Task = task, Question = q }).ToList();

                        context.Add(task);
                    }

                    if (task.AuthorId == null)
                        task.AuthorId = (await this.GetCurrentUserAsync()).Id;

                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExamTaskExists(taskInput.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Technologies", new { technologyId = taskInput.TechnologyId });
            }

            // restore vm information fields which were not posted
            taskInput.TechnologyName = (await this.context.Technology.SingleOrDefaultAsync(t => t.Id == taskInput.TechnologyId)).Name;

            var questions = this.context.TaskQuestion.Where(q => q.TechnologyId == taskInput.TechnologyId).ToList();
            foreach (var q in taskInput.Questions)
            {
                var original = questions.FirstOrDefault(x => x.Id == q.Id);
                if (original != null)
                    q.Text = original.Text;                               
            }
            
            return View(taskInput);
        }

        // GET: ExamTasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var examTask = await context.ExamTask
                .Include(e => e.Technology)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (examTask == null)
            {
                return NotFound();
            }

            return View(examTask);
        }

        // POST: ExamTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var examTask = await context.ExamTask.SingleOrDefaultAsync(m => m.Id == id);
            context.ExamTask.Remove(examTask);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ExamTaskExists(int id)
        {
            return context.ExamTask.Any(e => e.Id == id);
        }
    }
}
