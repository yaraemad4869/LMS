using LearningManagementSystem.Data;
using LearningManagementSystem.IRepo;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Repo
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Payment>> GetPaymentsByCourseAsync(int courseId)
        {
            return await _context.Payments
                .Include(p => p.Student)
                .Where(p => p.CourseId == courseId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }
        public async Task<IEnumerable<Payment>> GetPaymentsByStudentAsync(int studentId)
        {
            return await _context.Payments
                .Include(p => p.Course)
                .Where(p => p.StudentId == studentId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }
    }
}
