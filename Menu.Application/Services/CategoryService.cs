using AutoMapper;

using Menu.Application.Common.Exceptions;
using Menu.Application.Common.Models;
using Menu.Application.DTOs.Category;
using Menu.Application.Interfaces;
using Menu.Domain.Entities;
using Menu.Domain.Interfaces;

namespace Menu.Application.Services;

public class CategoryService : CrudServiceBase<Menu.Domain.Entities.Category, CategoryDto, CreateCategoryDto, UpdateCategoryDto>, ICategoryService
{
    private readonly ITenantContext _tenantContext;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, ITenantContext tenantContext)
        : base(unitOfWork, mapper, unitOfWork.Categories)
    {
        _tenantContext = tenantContext;
    }

    public override Task<PaginatedResult<CategoryDto>> GetAllAsync(QueryParameters parameters, CancellationToken ct = default)
    {
        var query =
            from category in Repository.Query()
            join menu in UnitOfWork.Menus.Query() on category.MenuId equals menu.Id
            select new CategoryDto(category.Id, category.Name, category.NameAr, category.DisplayOrder, category.MenuId, category.CreatedAt, menu.Name);

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var search = parameters.SearchTerm.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Name.ToLower().Contains(search) ||
                (x.NameAr ?? string.Empty).ToLower().Contains(search) ||
                (x.MenuName ?? string.Empty).ToLower().Contains(search));
        }

        var totalCount = query.Count();
        var data = query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        return Task.FromResult(new PaginatedResult<CategoryDto>
        {
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount,
            Data = data
        });
    }

    public override Task<CategoryDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = (
            from category in Repository.Query(ignoreQueryFilters: true)
            join menu in UnitOfWork.Menus.Query(ignoreQueryFilters: true) on category.MenuId equals menu.Id
            where category.Id == id
            select new { category, menu.RestaurantId, menu.Name })
            .FirstOrDefault();

        if (entity is null)
        {
            throw new NotFoundException($"{nameof(Category)} '{id}' was not found.");
        }

        EnsureRestaurantAccess(entity.RestaurantId);

        var dto = (
            from category in Repository.Query()
            join menu in UnitOfWork.Menus.Query() on category.MenuId equals menu.Id
            where category.Id == id
            select new CategoryDto(category.Id, category.Name, category.NameAr, category.DisplayOrder, category.MenuId, category.CreatedAt, menu.Name))
            .FirstOrDefault();

        if (dto is null)
        {
            throw new NotFoundException($"{nameof(Menu.Domain.Entities.Category)} '{id}' was not found.");
        }

        return Task.FromResult(dto);
    }

    public override async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default)
    {
        var menu = UnitOfWork.Menus.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == dto.MenuId);
        if (menu is null)
        {
            throw new NotFoundException($"Menu '{dto.MenuId}' was not found.");
        }

        EnsureRestaurantAccess(menu.RestaurantId);

        var entity = new Menu.Domain.Entities.Category
        {
            Name = dto.Name,
            NameAr = dto.NameAr,
            DisplayOrder = dto.DisplayOrder,
            MenuId = dto.MenuId
        };

        await Repository.AddAsync(entity, ct);
        await UnitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct);
    }

    public override async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken ct = default)
    {
        var entity = Repository.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == id);
        if (entity is null)
        {
            throw new NotFoundException($"{nameof(Category)} '{id}' was not found.");
        }

        var menu = UnitOfWork.Menus.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == entity.MenuId);
        if (menu is null)
        {
            throw new NotFoundException($"Menu '{entity.MenuId}' was not found.");
        }

        EnsureRestaurantAccess(menu.RestaurantId);

        entity.Name = dto.Name;
        entity.NameAr = dto.NameAr;
        entity.DisplayOrder = dto.DisplayOrder;

        Repository.Update(entity);
        await UnitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public override async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = Repository.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == id);
        if (entity is null)
        {
            throw new NotFoundException($"{nameof(Category)} '{id}' was not found.");
        }

        var menu = UnitOfWork.Menus.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == entity.MenuId);
        if (menu is null)
        {
            throw new NotFoundException($"Menu '{entity.MenuId}' was not found.");
        }

        EnsureRestaurantAccess(menu.RestaurantId);

        Repository.Delete(entity);
        await UnitOfWork.SaveChangesAsync(ct);
    }

    private void EnsureRestaurantAccess(Guid restaurantId)
    {
        if (!_tenantContext.CanAccessRestaurant(restaurantId))
        {
            throw new ForbiddenException("You do not have access to this restaurant.");
        }
    }
}
