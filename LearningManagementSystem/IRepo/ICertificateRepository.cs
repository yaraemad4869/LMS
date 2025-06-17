using LearningManagementSystem.Models;
using LearningManagementSystem.Repo;

namespace LearningManagementSystem.IRepo
{
    public interface ICertificateRepository : IRepository<Certificate>
    {
        Task<Certificate> GetByEnrollmentIdAsync(int enrollmentId);
        Task<Certificate> GetByVerificationCodeAsync(string verificationCode);
    }
}
