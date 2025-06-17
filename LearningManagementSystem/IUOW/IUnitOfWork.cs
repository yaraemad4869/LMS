using System.Threading.Tasks; 
using LearningManagementSystem.IRepo;

namespace LearningManagementSystem.IUOW
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        ICourseRepository Courses { get; }
        IModuleRepository Modules { get; }
        ILectureRepository Lectures { get; }
        IQuizRepository Quizzes { get; }
        IQuizAttemptRepository QuizAttempts { get; }
        IEnrollmentRepository Enrollments { get; }
        IProgressRepository Progress { get; }
        ICertificateRepository Certificates { get; }
        IReviewRepository Reviews { get; }
        IPaymentRepository Payments { get; }
        ISubscriptionRepository Subscriptions { get; }

        Task<int> CompleteAsync();
    }
}
