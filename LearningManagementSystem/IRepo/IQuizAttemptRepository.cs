using LearningManagementSystem.Models;
using LearningManagementSystem.Repo;

namespace LearningManagementSystem.IRepo
{
    public interface IQuizAttemptRepository : IRepository<QuizAttempt>
    {
        Task<IEnumerable<QuizAttempt>> GetAttemptsByStudentAsync(int studentId, int quizId);

    }
}
