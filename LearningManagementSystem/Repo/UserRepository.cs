using LearningManagementSystem.Data.Enum;
using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using LearningManagementSystem.IRepo;

namespace LearningManagementSystem.Repo
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<IEnumerable<User>> GetInstructorsAsync()
        {
            return await _dbSet
                .Where(u => u.UserRole.Name == "Instructor" && u.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetStudentsAsync()
        {
            return await _dbSet
                .Where(u => u.UserRole.Name == "Student" && u.IsActive)
                .ToListAsync();
        }
    }
}
