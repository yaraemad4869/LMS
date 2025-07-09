using LearningManagementSystem.Repo;
using LearningManagementSystem.Models;

namespace LearningManagementSystem.IRepo
{
    public interface IModuleRepository : IRepository<Module>
    {
        Task<IEnumerable<Module>> GetModulesByCourseAsync(int courseId);

    }
}
