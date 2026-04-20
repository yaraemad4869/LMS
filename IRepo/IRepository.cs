using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;
using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repo
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity> GetByIdAsync(int id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> AddAsync(TEntity entity);
        Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities);
        Task<TEntity> Update(TEntity entity);
        Task<TEntity> Remove(TEntity entity);
        Task<IEnumerable<TEntity>> RemoveRange(IEnumerable<TEntity> entities);
        Task<int> SaveChangesAsync();
    }
}
