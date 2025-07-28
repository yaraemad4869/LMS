using LearningManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, int>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        #region   DBSets
        public DbSet<User> Users { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Lecture> Lectures { get; set; }
        public DbSet<Order> Orders { get; set; }
        //public DbSet<Quiz> Quizzes { get; set; }
        //public DbSet<Question> Questions { get; set; }
        //public DbSet<Answer> Answers { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Progress> Progresses { get; set; }
        //public DbSet<QuizAttempt> QuizAttempts { get; set; }
        //public DbSet<QuizAnswer> QuizAnswers { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        #endregion
        public DbSet<LogEntry> LogEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LogEntry>(entity =>
            {
                entity.Property(e => e.Datetime).IsRequired();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Datetime);
                entity.HasIndex(e => new { e.UserId, e.Datetime });
            });
            modelBuilder.Entity<User>().Property(o => o.FullName)
        .HasComputedColumnSql("[FirstName] + ' ' + [LastName]");
            modelBuilder.Entity<IdentityUserLogin<int>>().HasKey(l => new { l.LoginProvider, l.ProviderKey });
            modelBuilder.Entity<IdentityUserRole<int>>().HasKey(r => new { r.UserId, r.RoleId });
            modelBuilder.Entity<IdentityUserToken<int>>().HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
            #region Relationships
            // User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            // Course
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Instructor)
                .WithMany()
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Module
            modelBuilder.Entity<Module>()
                .HasOne(m => m.Course)
                .WithMany(c => c.Modules)
                .HasForeignKey(m => m.CourseId);

            // Lecture
            modelBuilder.Entity<Lecture>()
                .HasOne(l => l.Module)
                .WithMany(m => m.Lectures)
                .HasForeignKey(l => l.ModuleId);

            // Quiz
            //modelBuilder.Entity<Quiz>()
            //    .HasOne(q => q.Lecture)
            //    .WithMany()
            //    .HasForeignKey(q => q.LectureId);

            // Question
            //modelBuilder.Entity<Question>()
            //    .HasOne(q => q.Quiz)
            //    .WithMany(q => q.Questions)
            //    .HasForeignKey(q => q.QuizId);

            // Answer
            //modelBuilder.Entity<Answer>()
            //    .HasOne(a => a.Question)
            //    .WithMany(q => q.Answers)
            //    .HasForeignKey(a => a.QuestionId);

            // Enrollment
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Progress
            modelBuilder.Entity<Progress>()
                .HasOne(p => p.Enrollment)
                .WithMany(e => e.Progress)
                .HasForeignKey(p => p.EnrollmentId);

            modelBuilder.Entity<Progress>()
                .HasOne(p => p.Lecture)
                .WithMany()
                .HasForeignKey(p => p.LectureId)
                .OnDelete(DeleteBehavior.Restrict);

            // QuizAttempt
            //modelBuilder.Entity<QuizAttempt>()
            //    .HasOne(qa => qa.Student)
            //    .WithMany()
            //    .HasForeignKey(qa => qa.StudentId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<QuizAttempt>()
            //    .HasOne(qa => qa.Quiz)
            //    .WithMany()
            //    .HasForeignKey(qa => qa.QuizId)
            //    .OnDelete(DeleteBehavior.Restrict);

            // QuizAnswer
            //modelBuilder.Entity<QuizAnswer>()
            //    .HasOne(qa => qa.QuizAttempt)
            //    .WithMany(qa => qa.Answers)
            //    .HasForeignKey(qa => qa.QuizAttemptId);

            //modelBuilder.Entity<QuizAnswer>()
            //    .HasOne(qa => qa.Question)
            //    .WithMany()
            //    .HasForeignKey(qa => qa.QuestionId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<QuizAnswer>()
            //    .HasOne(qa => qa.SelectedAnswer)
            //    .WithMany()
            //    .HasForeignKey(qa => qa.SelectedAnswerId)
                //.OnDelete(DeleteBehavior.Restrict);

            // Certificate
            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.Enrollment)
                .WithMany()
                .HasForeignKey(c => c.EnrollmentId);

            // Review
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Course)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CourseId);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Student)
                .WithMany()
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Course)
                .WithMany()
                .HasForeignKey(p => p.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Subscription
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserRole)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region Seed Data
            modelBuilder.Entity<Role>().HasData(
            new Role { Id = -1, Name = "Admin", NormalizedName = "ADMIN" },
            new Role { Id = -2, Name = "Instructor", NormalizedName = "INSTRUCTOR" },
            new Role { Id = -3, Name = "Student", NormalizedName = "STUDENT" }
            );
            #endregion

            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
