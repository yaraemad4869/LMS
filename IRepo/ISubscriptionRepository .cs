using LearningManagementSystem.Models;
using LearningManagementSystem.Repo;

namespace LearningManagementSystem.IRepo
{
    public interface ISubscriptionRepository : IRepository<Subscription>
    {
        Task<Subscription> GetActiveSubscriptionForStudentAsync(int studentId);

    }
}
