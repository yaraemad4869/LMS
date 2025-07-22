using LearningManagementSystem.Data;
using LearningManagementSystem.IRepo;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Repo
{
    public class QuizRepository : Repository<Quiz>, IQuizRepository
    {
        public QuizRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Quiz> GetQuizByLectureIdAsync(int lectureId)
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.LectureId == lectureId);
        }

        public async Task<Quiz> GetQuizWithQuestionsAsync(int id)
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);
        }
    }
}
