using LearningManagementSystem.Data;
using LearningManagementSystem.IRepo;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Repo
{
    public class InstructorRepository : Repository<Instructor>, IInstructorRepository
    {
        public InstructorRepository(ApplicationDbContext context) : base(context) { }
        public async Task<Instructor?> GetInstructorByEmail(string email)
        {
            return await _context.Instructors.FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<Instructor?> GetInstructorByPhoneNumber(string phoneNumber)
        {
            return await _context.Instructors.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }

    }
}
