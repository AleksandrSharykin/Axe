using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Axe.Models;
using Axe.Models.ExamTasksVm;

namespace Axe.Managers
{
    public class ExamTaskManager : ManagerBase, IExamTaskManager
    {
        public ExamTaskManager(AxeDbContext context) : base(context) { }

        /// <summary>
        /// Gets <see cref="ExamTask"/> details for display
        /// </summary>
        public async Task<Response<ExamTask>> DetailsGet(Request<int?> request)
        {
            int? id = request.Item;

            if (id == null)
            {
                return this.NotFound<ExamTask>();
            }

            var examTask = await context.ExamTask
                .Include(e => e.Technology)
                .Include(e => e.Questions).ThenInclude(q => q.Question)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (examTask == null)
            {
                return this.NotFound<ExamTask>();
            }

            return this.Response(examTask);
        }

        /// <summary>
        /// Gets <see cref="ExamTask"/> object template for creation or edit
        /// </summary>
        public async Task<Response<TaskInputVm>> InputGet(Request<ExamTask> request)
        {
            int id = request.Item.Id;
            int? technologyId = request.Item.TechnologyId;

            ExamTask examTask = null;

            if (id > 0)
            {
                // edit existing exam task
                examTask = await context.ExamTask.Include(t => t.Technology)
                                                 .Include(t => t.Questions)
                                                 .SingleOrDefaultAsync(m => m.Id == id);
                if (examTask == null)
                {
                    return this.NotFound<TaskInputVm>();
                }
            }
            else
            {
                // create new exam task
                var tech = await this.context.Technology.SingleOrDefaultAsync(t => t.Id == technologyId);
                if (tech == null)
                {
                    return this.NotFound<TaskInputVm>();
                }

                examTask = new ExamTask()
                {
                    Technology = tech,
                    Questions = new List<TaskQuestionLink>(),
                };
            }

            // get all questions for selected technology for exam task 
            var questions = this.context.TaskQuestion.Where(q => q.TechnologyId == examTask.Technology.Id).ToList();

            var taskVm = new TaskInputVm
            {
                Id = examTask.Id,
                TechnologyName = examTask.Technology.Name,
                TechnologyId = examTask.Technology.Id,
                Title = examTask.Title,
                Objective = examTask.Objective,
                IsDemonstration = examTask.IsDemonstration,
                PassingThreshold = examTask.PassingThreshold,
                Questions = questions.Select(q => new QuestionSelectionVm
                {
                    Id = q.Id,
                    Text = q.Text,
                    Preview = q.Preview,
                    Type = q.Type,
                    IsSelected = examTask.Questions.Any(x => x.QuestionId == q.Id),
                }).ToList()
            };

            return this.Response(taskVm);
        }

        /// <summary>
        /// Applies <see cref="ExamTask"/> edit results
        /// </summary>
        public async Task<Response<TaskInputVm>> InputPost(Request<TaskInputVm> request)
        {
            TaskInputVm taskInput = request.Item;

            // questions selected for task
            List<int> ids = null;
            if (request.ModelState.IsValid)
            {
                ids = taskInput.Questions
                                .Where(q => q.IsSelected)
                                .Select(q => q.Id)
                                .ToList();

                if (ids.Count == 0)
                {
                    request.ModelState.AddModelError(String.Empty, ValidationMessages.Instance.TaskNoQuestions);
                }
            }

            Technology tech = null;
            if (request.ModelState.IsValid)
            {
                tech = this.context.Technology.Include(t => t.Experts)
                           .FirstOrDefault(t => t.Id == taskInput.TechnologyId);
                if (tech == null)
                {
                    request.ModelState.AddModelError(String.Empty, ValidationMessages.Instance.UnknownTechnology);
                }
                else if (false == tech.Experts.Any(t => t.UserId == request.CurrentUser.Id))
                {
                    request.ModelState.AddModelError(String.Empty, ValidationMessages.Instance.TaskExpertInput(tech.Name));
                }
            }

            if (request.ModelState.IsValid)
            {
                ExamTask task;

                if (taskInput.Id > 0)
                {
                    task = await this.context.ExamTask.Include(t => t.Questions)
                                     .SingleOrDefaultAsync(t => t.Id == taskInput.Id);

                    // update descriptive properties
                    task.Title = taskInput.Title;
                    task.Objective = taskInput.Objective;
                    task.IsDemonstration = taskInput.IsDemonstration;
                    task.PassingThreshold = taskInput.PassingThreshold;

                    // remove unselected questions
                    var deletedQuestion = task.Questions.Where(q => false == ids.Contains(q.QuestionId)).ToList();
                    foreach (var question in deletedQuestion)
                    {
                        task.Questions.Remove(question);
                    }

                    // add new selected questions 
                    var addedQuestions = ids.Where(id => false == task.Questions.Any(q => q.QuestionId == id)).ToList();
                    foreach (int questionId in addedQuestions)
                    {
                        task.Questions.Add(new TaskQuestionLink { Task = task, QuestionId = questionId });
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
                        IsDemonstration = taskInput.IsDemonstration,
                        PassingThreshold = taskInput.PassingThreshold,
                    };

                    var taskQuestions = this.context.TaskQuestion
                                            .Where(q => ids.Contains(q.Id))
                                            .ToList();

                    task.Questions = taskQuestions.Select(q => new TaskQuestionLink { Task = task, Question = q }).ToList();

                    context.Add(task);
                }

                if (task.AuthorId == null)
                {
                    task.AuthorId = request.CurrentUser.Id;
                }

                await context.SaveChangesAsync();

                return this.Response(new TaskInputVm() { TechnologyId = taskInput.TechnologyId });
            }

            // restore information fields which were not posted
            taskInput.TechnologyName = tech?.Name;

            // restore question list
            var questions = this.context.TaskQuestion.Where(q => q.TechnologyId == taskInput.TechnologyId).ToList();
            foreach (var q in taskInput.Questions)
            {
                var original = questions.FirstOrDefault(x => x.Id == q.Id);
                if (original != null)
                {
                    q.Type = original.Type;
                    q.Text = original.Text;
                    q.Preview = original.Preview;
                }
            }

            return this.ValidationError(taskInput);
        }

        /// <summary>
        /// Gets <see cref="ExamTask"/> object for preview before deletion
        /// </summary>
        public async Task<Response<ExamTask>> DeleteGet(Request<int> request)
        {
            var exam = await this.context.ExamTask
                         .Include(t => t.Technology).ThenInclude(t => t.Experts)
                         .SingleOrDefaultAsync(t => t.Id == request.Item);

            if (exam == null || false == exam.Technology.Experts.Any(u => u.UserId == request.CurrentUser.Id))
            {
                return this.NotFound<ExamTask>();
            }

            return this.Response(exam);
        }

        /// <summary>
        /// Deletes <see cref="ExamTask"/>
        /// </summary>
        public async Task<Response<ExamTask>> DeletePost(Request<int> request)
        {
            var exam = await this.context.ExamTask
                         .Include(t => t.Technology).ThenInclude(t => t.Experts)
                         .SingleOrDefaultAsync(t => t.Id == request.Item);

            if (exam == null)
            {
                return this.NotFound<ExamTask>();
            }

            if (false == exam.Technology.Experts.Any(u => u.UserId == request.CurrentUser.Id))
            {
                request.ModelState.AddModelError(String.Empty, ValidationMessages.Instance.TaskExpertDelete(exam.Technology.Name));
                return this.ValidationError(exam);
            }

            this.context.Remove(exam);
            await this.context.SaveChangesAsync();

            return this.Response(new ExamTask());
        }
    }
}
