using LearningManagementSystem.Models;
using LearningManagementSystem.Repo;

namespace LearningManagementSystem.IRepo
{
    public interface IQuizRepository : IRepository<Quiz>
    {
        Task<Quiz> GetQuizWithQuestionsAsync(int id);
        Task<Quiz> GetQuizByLectureIdAsync(int lectureId);
    }
}
