using LearningManagementSystem.Models;
using LearningManagementSystem.Repo;

namespace LearningManagementSystem.IRepo
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetPaymentsByStudentAsync(int studentId);
        Task<IEnumerable<Payment>> GetPaymentsByCourseAsync(int courseId);

    }
}
