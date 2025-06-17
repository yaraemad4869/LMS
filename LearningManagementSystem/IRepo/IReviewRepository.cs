using LearningManagementSystem.Models;
using LearningManagementSystem.Repo;

namespace LearningManagementSystem.IRepo
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<IEnumerable<Review>> GetReviewsByCourseAsync(int courseId);
        Task<Review> GetReviewByStudentAndCourseAsync(int studentId, int courseId);
        Task<double> GetAverageRatingForCourseAsync(int courseId);
    }
}
