using LearningManagementSystem.Repo;
using System.Reflection;

namespace LearningManagementSystem.IRepo
{
    public interface IModuleRepository : IRepository<Module>
    {
        Task<IEnumerable<Module>> GetModulesByCourseAsync(int courseId);

    }
}
