using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Axe.Models
{
    public class AxeDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Technology> Technology { get; set; }

        public DbSet<SkillAssessment> SkillAssessment { get; set; }

        public DbSet<ExamTask> ExamTask { get; set; }

        public DbSet<TaskQuestion> TaskQuestion { get; set; }

        public DbSet<TaskAnswer> TaskAnswer { get; set; }

        public DbSet<ExamAttempt> ExamAttempt { get; set; }

        public DbSet<AttemptQuestion> AttemptQuestion { get; set; }

        public DbSet<AttemptAnswer> AttemptAnswer { get; set; }        

        public AxeDbContext (DbContextOptions<AxeDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {            
            builder.Entity<SkillAssessment>().HasOne(a => a.Student).WithMany(u => u.AssessmentsAsStudent);
            builder.Entity<SkillAssessment>().HasOne(a => a.Examiner).WithMany(u => u.AssessmentsAsExaminer);

            builder.Entity<ExamAttempt>().HasOne(ea => ea.Task).WithMany().OnDelete(DeleteBehavior.Restrict);
            builder.Entity<AttemptQuestion>().HasOne(q => q.TaskQuestion).WithMany().OnDelete(DeleteBehavior.Restrict);
            builder.Entity<AttemptAnswer>().HasOne(q => q.TaskAnswer).WithMany().OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(builder);
        }
    }
}
