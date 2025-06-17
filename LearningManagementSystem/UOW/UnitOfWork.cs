using LearningManagementSystem.Data;
using LearningManagementSystem.IRepo;
using LearningManagementSystem.Models;
using LearningManagementSystem.Repo;
using LearningManagementSystem.IUOW; 

namespace LearningManagementSystem.UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Users = new UserRepository(_context);
            Courses = new CourseRepository(_context);
            Modules = new ModuleRepository(_context);
            Lectures = new LectureRepository(_context);
            Quizzes = new QuizRepository(_context);
            QuizAttempts = new QuizAttemptRepository(_context);
            Enrollments = new EnrollmentRepository(_context);
            Progress = new ProgressRepository(_context);
            Certificates = new CertificateRepository(_context);
            Reviews = new ReviewRepository(_context);
            Payments = new PaymentRepository(_context);
            Subscriptions = new SubscriptionRepository(_context);
        }

        public IUserRepository Users { get; private set; }
        public ICourseRepository Courses { get; private set; }
        public IModuleRepository Modules { get; private set; }
        public ILectureRepository Lectures { get; private set; }
        public IQuizRepository Quizzes { get; private set; }
        public IQuizAttemptRepository QuizAttempts { get; private set; }
        public IEnrollmentRepository Enrollments { get; private set; }
        public IProgressRepository Progress { get; private set; }
        public ICertificateRepository Certificates { get; private set; }
        public IReviewRepository Reviews { get; private set; }
        public IPaymentRepository Payments { get; private set; }
        public ISubscriptionRepository Subscriptions { get; private set; }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
