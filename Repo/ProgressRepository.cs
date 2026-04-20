using LearningManagementSystem.Data;
using LearningManagementSystem.IRepo;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Repo
{
    public class ProgressRepository : Repository<Progress>, IProgressRepository
    {
        public ProgressRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<Progress> GetProgressAsync(int enrollmentId, int lectureId)
        {
            return await _context.Progresses
                .FirstOrDefaultAsync(p => p.EnrollmentId == enrollmentId && p.LectureId == lectureId);
        }

        public async Task<IEnumerable<Progress>> GetProgressByEnrollmentAsync(int enrollmentId)
        {
            return await _context.Progresses
                .Where(p => p.EnrollmentId == enrollmentId)
                .ToListAsync();
        }
    }
}
