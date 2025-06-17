using LearningManagementSystem.Data;
using LearningManagementSystem.IRepo;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Repo
{
    public class CertificateRepository : Repository<Certificate>, ICertificateRepository
    {
        public CertificateRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<Certificate> GetByEnrollmentIdAsync(int enrollmentId)
        {
            return await _context.Certificates
                .FirstOrDefaultAsync(c => c.EnrollmentId == enrollmentId);
        }

        public async Task<Certificate> GetByVerificationCodeAsync(string verificationCode)
        {
            return await _context.Certificates
                .Include(c => c.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(c => c.Enrollment)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(c => c.VerificationCode == verificationCode);
        }
    }
}
