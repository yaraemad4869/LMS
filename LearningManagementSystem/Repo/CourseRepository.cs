using LearningManagementSystem.Data;
using LearningManagementSystem.IRepo;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Repo
{
    public class CourseRepository :Repository<Course>, ICourseRepository
    {
        public CourseRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<Course> GetCourseWithDetailsAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Modules.OrderBy(m => m.Order))
                    .ThenInclude(m => m.Lectures.OrderBy(l => l.Order))
                .Include(c => c.Reviews)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        public async Task<IEnumerable<Course>> GetCoursesByInstructorAsync(int instructorId)
        {
            return await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Reviews)
                .Where(c => c.InstructorId == instructorId)
                .ToListAsync();
        }
        public async Task<IEnumerable<Course>> GetNewestCoursesAsync(int count)
        {
            return await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Reviews)
                .Where(c => c.IsPublished)
                .OrderByDescending(c => c.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
        public async Task<IEnumerable<Course>> GetPopularCoursesAsync(int count)
        {
            return await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Reviews)
                .Include(c => c.Enrollments)
                .Where(c => c.IsPublished)
                .OrderByDescending(c => c.Enrollments.Count)
                .Take(count)
                .ToListAsync();
        }
        public async Task<IEnumerable<Course>> SearchCoursesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await _context.Courses
                    .Include(c => c.Instructor)
                    .Include(c => c.Reviews)
                    .Where(c => c.IsPublished)
                    .ToListAsync();
            }

            searchTerm = searchTerm.ToLower();

            return await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Reviews)
                .Where(c => c.IsPublished &&
                    (c.Title.ToLower().Contains(searchTerm) ||
                     c.Description.ToLower().Contains(searchTerm) ||
                     c.Instructor.FirstName.ToLower().Contains(searchTerm) ||
                     c.Instructor.LastName.ToLower().Contains(searchTerm)))
                .ToListAsync();
        }

    }
}
