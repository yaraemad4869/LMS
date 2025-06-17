using LearningManagementSystem.Data;
using LearningManagementSystem.IRepo;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Repo
{
    public class QuizAttemptRepository : Repository<QuizAttempt>, IQuizAttemptRepository
    {
        public QuizAttemptRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<QuizAttempt>> GetAttemptsByStudentAsync(int studentId, int quizId)
        {
            return await _context.QuizAttempts
                .Include(qa => qa.Answers)
                .Where(qa => qa.StudentId == studentId && qa.QuizId == quizId)
                .OrderByDescending(qa => qa.StartTime)
                .ToListAsync();
        }
    }
}
