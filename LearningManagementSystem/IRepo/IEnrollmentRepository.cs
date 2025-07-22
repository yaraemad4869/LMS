using LearningManagementSystem.Models;
using LearningManagementSystem.Repo;

namespace LearningManagementSystem.IRepo
{
    public interface IEnrollmentRepository : IRepository<Enrollment>
    {
        Task<Enrollment> GetEnrollmentAsync(int studentId, int courseId);
        Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentAsync(int studentId);
        Task<IEnumerable<Enrollment>> GetEnrollmentsByCourseAsync(int courseId);
    }
}
