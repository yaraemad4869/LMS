using LearningManagementSystem.Data;
using LearningManagementSystem.IRepo;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Repo
{
    public class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
    {
        public SubscriptionRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<Subscription> GetActiveSubscriptionForStudentAsync(int studentId)
        {
            return await _context.Subscriptions
                .FirstOrDefaultAsync(s =>
                    s.StudentId == studentId &&
                    s.IsActive &&
                    s.EndDate > DateTime.UtcNow);
        }
    }
}
