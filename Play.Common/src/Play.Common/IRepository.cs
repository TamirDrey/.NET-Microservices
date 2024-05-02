using System.Linq.Expressions;

namespace Play.Common
{
    public interface IRepository<T> where T : IEntity
    {
        Task CreateAsync(T entiy);
        Task<IReadOnlyCollection<T>> GetAllAsync();
        Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T,bool>> filter);
        Task<T> GetAsynq(Guid id);
        Task<T> GetAsynq(Expression<Func<T,bool>> filter);
        Task RemoveAsync(Guid id);
        Task UpdateAsync(T entiy);
    }
}