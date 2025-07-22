using LearningManagementSystem.Models;
using LearningManagementSystem.Repo;

namespace LearningManagementSystem.IRepo
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<Course> GetCourseWithDetailsAsync(int id);
        Task<IEnumerable<Course>> GetCoursesByInstructorAsync(int instructorId);
        Task<IEnumerable<Course>> SearchCoursesAsync(string searchTerm);
        Task<IEnumerable<Course>> GetPopularCoursesAsync(int count);
        Task<IEnumerable<Course>> GetNewestCoursesAsync(int count);
    }
}
