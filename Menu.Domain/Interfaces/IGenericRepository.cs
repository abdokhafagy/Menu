using System.Linq.Expressions;

using Menu.Domain.Entities;

namespace Menu.Domain.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<T?> GetByIdAsync(Guid id, Func<IQueryable<T>, IQueryable<T>>? include, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    IQueryable<T> Query(bool ignoreQueryFilters = false);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Delete(T entity);
    void HardDelete(T entity);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
}