using LearningManagementSystem.Models;
using LearningManagementSystem.Models.DTOs;
using LearningManagementSystem.Repo;

namespace LearningManagementSystem.IRepo
{
    public interface ICertificateRepository : IRepository<Certificate>
    {
        //Task<Certificate> GetByEnrollmentIdAsync(int enrollmentId);
        //Task<Certificate> GetByVerificationCodeAsync(string verificationCode);
        Task<byte[]> GenerateCertificatePdf(int enrollmentId);
        Task<List<CertificateDto>> GetCertificatesByStudent(int studentId);
    }
}
