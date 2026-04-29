using AutoMapper;

using Menu.Application.Common.Exceptions;
using Menu.Application.Common.Models;
using Menu.Application.DTOs.MenuItem;
using Menu.Application.Interfaces;
using Menu.Domain.Entities;
using Menu.Domain.Interfaces;

namespace Menu.Application.Services;

public class MenuItemService : CrudServiceBase<Menu.Domain.Entities.MenuItem, MenuItemDto, CreateMenuItemDto, UpdateMenuItemDto>, IMenuItemService
{
    private readonly ITenantContext _tenantContext;

    public MenuItemService(IUnitOfWork unitOfWork, IMapper mapper, ITenantContext tenantContext)
        : base(unitOfWork, mapper, unitOfWork.MenuItems)
    {
        _tenantContext = tenantContext;
    }

    public override Task<PaginatedResult<MenuItemDto>> GetAllAsync(QueryParameters parameters, CancellationToken ct = default)
    {
        var query =
            from item in Repository.Query()
            join category in UnitOfWork.Categories.Query() on item.CategoryId equals category.Id
            select new MenuItemDto(
                item.Id,
                item.Name,
                item.NameAr,
                item.Description,
                item.DescriptionAr,
                item.Price,
                item.ImageUrl,
                item.IsAvailable,
                item.DisplayOrder,
                item.CategoryId,
                item.CreatedAt,
                category.Name);

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var search = parameters.SearchTerm.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Name.ToLower().Contains(search) ||
                (x.NameAr ?? string.Empty).ToLower().Contains(search) ||
                (x.Description ?? string.Empty).ToLower().Contains(search) ||
                (x.CategoryName ?? string.Empty).ToLower().Contains(search));
        }

        var totalCount = query.Count();
        var data = query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        return Task.FromResult(new PaginatedResult<MenuItemDto>
        {
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount,
            Data = data
        });
    }

    public override Task<MenuItemDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = (
            from item in Repository.Query(ignoreQueryFilters: true)
            join category in UnitOfWork.Categories.Query(ignoreQueryFilters: true) on item.CategoryId equals category.Id
            join menu in UnitOfWork.Menus.Query(ignoreQueryFilters: true) on category.MenuId equals menu.Id
            where item.Id == id
            select new { item, menu.RestaurantId, category.Name })
            .FirstOrDefault();

        if (entity is null)
        {
            throw new NotFoundException($"{nameof(MenuItem)} '{id}' was not found.");
        }

        EnsureRestaurantAccess(entity.RestaurantId);

        var dto = (
            from item in Repository.Query()
            join category in UnitOfWork.Categories.Query() on item.CategoryId equals category.Id
            where item.Id == id
            select new MenuItemDto(
                item.Id,
                item.Name,
                item.NameAr,
                item.Description,
                item.DescriptionAr,
                item.Price,
                item.ImageUrl,
                item.IsAvailable,
                item.DisplayOrder,
                item.CategoryId,
                item.CreatedAt,
                category.Name))
            .FirstOrDefault();

        if (dto is null)
        {
            throw new NotFoundException($"{nameof(MenuItem)} '{id}' was not found.");
        }

        return Task.FromResult(dto);
    }

    public override async Task<MenuItemDto> CreateAsync(CreateMenuItemDto dto, CancellationToken ct = default)
    {
        var category = UnitOfWork.Categories.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == dto.CategoryId);
        if (category is null)
        {
            throw new NotFoundException($"Category '{dto.CategoryId}' was not found.");
        }

        var menu = UnitOfWork.Menus.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == category.MenuId);
        if (menu is null)
        {
            throw new NotFoundException($"Menu '{category.MenuId}' was not found.");
        }

        EnsureRestaurantAccess(menu.RestaurantId);

        var entity = new Menu.Domain.Entities.MenuItem
        {
            Name = dto.Name,
            NameAr = dto.NameAr,
            Description = dto.Description,
            DescriptionAr = dto.DescriptionAr,
            Price = dto.Price,
            ImageUrl = dto.ImageUrl,
            IsAvailable = dto.IsAvailable,
            DisplayOrder = dto.DisplayOrder,
            CategoryId = dto.CategoryId
        };

        await Repository.AddAsync(entity, ct);
        await UnitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct);
    }

    public override async Task<MenuItemDto> UpdateAsync(Guid id, UpdateMenuItemDto dto, CancellationToken ct = default)
    {
        var entity = Repository.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == id);
        if (entity is null)
        {
            throw new NotFoundException($"{nameof(MenuItem)} '{id}' was not found.");
        }

        var category = UnitOfWork.Categories.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == entity.CategoryId);
        if (category is null)
        {
            throw new NotFoundException($"Category '{entity.CategoryId}' was not found.");
        }

        var menu = UnitOfWork.Menus.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == category.MenuId);
        if (menu is null)
        {
            throw new NotFoundException($"Menu '{category.MenuId}' was not found.");
        }

        EnsureRestaurantAccess(menu.RestaurantId);

        entity.Name = dto.Name;
        entity.NameAr = dto.NameAr;
        entity.Description = dto.Description;
        entity.DescriptionAr = dto.DescriptionAr;
        entity.Price = dto.Price;
        entity.ImageUrl = dto.ImageUrl;
        entity.IsAvailable = dto.IsAvailable;
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
            throw new NotFoundException($"{nameof(MenuItem)} '{id}' was not found.");
        }

        var category = UnitOfWork.Categories.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == entity.CategoryId);
        if (category is null)
        {
            throw new NotFoundException($"Category '{entity.CategoryId}' was not found.");
        }

        var menu = UnitOfWork.Menus.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == category.MenuId);
        if (menu is null)
        {
            throw new NotFoundException($"Menu '{category.MenuId}' was not found.");
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
