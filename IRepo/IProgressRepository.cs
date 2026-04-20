using LearningManagementSystem.Models;
using LearningManagementSystem.Repo;

namespace LearningManagementSystem.IRepo
{
    public interface IProgressRepository : IRepository<Progress>
    {
        Task<Progress> GetProgressAsync(int enrollmentId, int lectureId);
        Task<IEnumerable<Progress>> GetProgressByEnrollmentAsync(int enrollmentId);
    }
}
