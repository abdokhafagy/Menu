using System.Linq.Expressions;

using Menu.Domain.Entities;
using Menu.Domain.Interfaces;
using Menu.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menu.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _dbSet.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<T?> GetByIdAsync(Guid id, Func<IQueryable<T>, IQueryable<T>>? include, CancellationToken ct = default)
    {
        IQueryable<T> query = _dbSet;
        if (include is not null)
        {
            query = include(query);
        }

        return await query.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(ct);
    }

    public IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }

    public Task AddAsync(T entity, CancellationToken ct = default)
    {
        return _dbSet.AddAsync(entity, ct).AsTask();
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Delete(T entity)
    {
        entity.IsDeleted = true;
        _dbSet.Update(entity);
    }

    public void HardDelete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
    {
        return predicate is null ? _dbSet.CountAsync(ct) : _dbSet.CountAsync(predicate, ct);
    }

    public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return _dbSet.AnyAsync(predicate, ct);
    }
}
