using AutoMapper;

using Menu.Application.Common.Exceptions;
using Menu.Application.Common.Models;
using Menu.Application.Interfaces;
using Menu.Domain.Entities;
using Menu.Domain.Interfaces;

namespace Menu.Application.Services;

public abstract class CrudServiceBase<TEntity, TDto, TCreateDto, TUpdateDto> : ICrudService<TDto, TCreateDto, TUpdateDto>
    where TEntity : BaseEntity
{
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly IMapper Mapper;
    protected readonly IGenericRepository<TEntity> Repository;

    protected CrudServiceBase(IUnitOfWork unitOfWork, IMapper mapper, IGenericRepository<TEntity> repository)
    {
        UnitOfWork = unitOfWork;
        Mapper = mapper;
        Repository = repository;
    }

    protected virtual IQueryable<TEntity> BuildQuery()
    {
        return Repository.Query();
    }

    protected virtual Func<IQueryable<TEntity>, IQueryable<TEntity>>? GetByIdIncludes()
    {
        return null;
    }

    public virtual async Task<TDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var include = GetByIdIncludes();
        var entity = include is null
            ? await Repository.GetByIdAsync(id, ct)
            : await Repository.GetByIdAsync(id, include, ct);

        if (entity is null)
        {
            throw new NotFoundException($"{typeof(TEntity).Name} '{id}' was not found.");
        }

        return Mapper.Map<TDto>(entity);
    }

    public virtual Task<PaginatedResult<TDto>> GetAllAsync(QueryParameters parameters, CancellationToken ct = default)
    {
        var query = BuildQuery();

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var search = parameters.SearchTerm.Trim().ToLowerInvariant();
            query = query.Where(e => e.ToString() != null && e.ToString()!.ToLower().Contains(search));
        }

        var totalCount = query.Count();
        var data = query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        return Task.FromResult(new PaginatedResult<TDto>
        {
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount,
            Data = Mapper.Map<IReadOnlyList<TDto>>(data)
        });
    }

    public virtual async Task<TDto> CreateAsync(TCreateDto dto, CancellationToken ct = default)
    {
        var entity = Mapper.Map<TEntity>(dto);
        await Repository.AddAsync(entity, ct);
        await UnitOfWork.SaveChangesAsync(ct);
        return Mapper.Map<TDto>(entity);
    }

    public virtual async Task<TDto> UpdateAsync(Guid id, TUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await Repository.GetByIdAsync(id, ct);
        if (entity is null)
        {
            throw new NotFoundException($"{typeof(TEntity).Name} '{id}' was not found.");
        }

        Mapper.Map(dto, entity);
        Repository.Update(entity);
        await UnitOfWork.SaveChangesAsync(ct);
        return Mapper.Map<TDto>(entity);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await Repository.GetByIdAsync(id, ct);
        if (entity is null)
        {
            throw new NotFoundException($"{typeof(TEntity).Name} '{id}' was not found.");
        }

        Repository.Delete(entity);
        await UnitOfWork.SaveChangesAsync(ct);
    }
}
