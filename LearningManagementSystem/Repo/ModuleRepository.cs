using LearningManagementSystem.Data;
using LearningManagementSystem.IRepo;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace LearningManagementSystem.Repo
{
    public class ModuleRepository : Repository<Module>, IModuleRepository
    {
        public ModuleRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Module>> GetModulesByCourseAsync(int courseId)
        {
            var modules = await _context.Modules
                   .Include(m => m.Lectures)
                   .Where(m => m.CourseId == courseId)
                   .OrderBy(m => m.Order)
                   .ToListAsync();

            foreach (var module in modules)
            {
                module.Lectures = module.Lectures.OrderBy(l => l.Order).ToList();
            }

            return (IEnumerable<Module>)modules;

        }
    }
}
