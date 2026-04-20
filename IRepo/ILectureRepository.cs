using LearningManagementSystem.Models;
using LearningManagementSystem.Repo;

namespace LearningManagementSystem.IRepo
{
    public interface ILectureRepository : IRepository<Lecture>
    {
        Task<IEnumerable<Lecture>> GetLecturesByModuleAsync(int moduleId);

    }
}
