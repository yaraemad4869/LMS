using LearningManagementSystem.Models;
using LearningManagementSystem.Repo;

namespace LearningManagementSystem.IRepo
{
    public interface IInstructorRepository : IRepository<Instructor>
    {
        Task<Instructor?> GetInstructorByEmail(string email);
        Task<Instructor?> GetInstructorByPhoneNumber(string phoneNumber);
    }
}
