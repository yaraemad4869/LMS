using LearningManagementSystem.Data;
using LearningManagementSystem.IRepo;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Repo
{
    public class EnrollmentRepository : Repository<Enrollment>, IEnrollmentRepository
    {
        public EnrollmentRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<Enrollment> GetEnrollmentAsync(int studentId, int courseId)
        {
            return await _context.Enrollments
                .Include(e => e.Progress)
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);
        }
        public async Task<IEnumerable<Enrollment>> GetEnrollmentsByCourseAsync(int courseId)
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentAsync(int studentId)
        {
            return await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                .Include(e => e.Progress)
                .Where(e => e.StudentId == studentId)
                .ToListAsync();
        }

    }
}
