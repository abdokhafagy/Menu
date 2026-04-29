using AutoMapper;

using Menu.Application.Common.Exceptions;
using Menu.Application.Common.Models;
using Menu.Application.DTOs.ItemOption;
using Menu.Application.Interfaces;
using Menu.Domain.Entities;
using Menu.Domain.Interfaces;

namespace Menu.Application.Services;

public class ItemOptionService : CrudServiceBase<ItemOption, ItemOptionDto, CreateItemOptionDto, UpdateItemOptionDto>, IItemOptionService
{
    private readonly ITenantContext _tenantContext;

    public ItemOptionService(IUnitOfWork unitOfWork, IMapper mapper, ITenantContext tenantContext)
        : base(unitOfWork, mapper, unitOfWork.ItemOptions)
    {
        _tenantContext = tenantContext;
    }

    public override Task<PaginatedResult<ItemOptionDto>> GetAllAsync(QueryParameters parameters, CancellationToken ct = default)
    {
        var query =
            from option in Repository.Query()
            join menuItem in UnitOfWork.MenuItems.Query() on option.MenuItemId equals menuItem.Id
            select new ItemOptionDto(
                option.Id,
                option.Name,
                option.NameAr,
                option.IsRequired,
                option.MinSelections,
                option.MaxSelections,
                option.SelectionType,
                option.MenuItemId,
                option.CreatedAt,
                menuItem.Name);

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var search = parameters.SearchTerm.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Name.ToLower().Contains(search) ||
                (x.NameAr ?? string.Empty).ToLower().Contains(search) ||
                (x.MenuItemName ?? string.Empty).ToLower().Contains(search));
        }

        var totalCount = query.Count();
        var data = query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        return Task.FromResult(new PaginatedResult<ItemOptionDto>
        {
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount,
            Data = data
        });
    }

    public override Task<ItemOptionDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = (
            from option in Repository.Query(ignoreQueryFilters: true)
            join menuItem in UnitOfWork.MenuItems.Query(ignoreQueryFilters: true) on option.MenuItemId equals menuItem.Id
            join category in UnitOfWork.Categories.Query(ignoreQueryFilters: true) on menuItem.CategoryId equals category.Id
            join menu in UnitOfWork.Menus.Query(ignoreQueryFilters: true) on category.MenuId equals menu.Id
            where option.Id == id
            select new { option, menu.RestaurantId, menuItem.Name })
            .FirstOrDefault();

        if (entity is null)
        {
            throw new NotFoundException($"{nameof(ItemOption)} '{id}' was not found.");
        }

        EnsureRestaurantAccess(entity.RestaurantId);

        var dto = (
            from option in Repository.Query()
            join menuItem in UnitOfWork.MenuItems.Query() on option.MenuItemId equals menuItem.Id
            where option.Id == id
            select new ItemOptionDto(
                option.Id,
                option.Name,
                option.NameAr,
                option.IsRequired,
                option.MinSelections,
                option.MaxSelections,
                option.SelectionType,
                option.MenuItemId,
                option.CreatedAt,
                menuItem.Name))
            .FirstOrDefault();

        if (dto is null)
        {
            throw new NotFoundException($"{nameof(ItemOption)} '{id}' was not found.");
        }

        return Task.FromResult(dto);
    }

    public override async Task<ItemOptionDto> CreateAsync(CreateItemOptionDto dto, CancellationToken ct = default)
    {
        var menuItem = UnitOfWork.MenuItems.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == dto.MenuItemId);
        if (menuItem is null)
        {
            throw new NotFoundException($"Menu item '{dto.MenuItemId}' was not found.");
        }

        var category = UnitOfWork.Categories.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == menuItem.CategoryId);
        var menu = category is null ? null : UnitOfWork.Menus.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == category.MenuId);
        if (menu is null)
        {
            throw new NotFoundException($"Menu '{menuItem.CategoryId}' was not found.");
        }

        EnsureRestaurantAccess(menu.RestaurantId);

        var entity = new ItemOption
        {
            Name = dto.Name,
            NameAr = dto.NameAr,
            IsRequired = dto.IsRequired,
            MinSelections = dto.MinSelections,
            MaxSelections = dto.MaxSelections,
            SelectionType = dto.SelectionType,
            MenuItemId = dto.MenuItemId
        };

        await Repository.AddAsync(entity, ct);
        await UnitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct);
    }

    public override async Task<ItemOptionDto> UpdateAsync(Guid id, UpdateItemOptionDto dto, CancellationToken ct = default)
    {
        var entity = Repository.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == id);
        if (entity is null)
        {
            throw new NotFoundException($"{nameof(ItemOption)} '{id}' was not found.");
        }

        var menuItem = UnitOfWork.MenuItems.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == entity.MenuItemId);
        if (menuItem is null)
        {
            throw new NotFoundException($"Menu item '{entity.MenuItemId}' was not found.");
        }

        var category = UnitOfWork.Categories.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == menuItem.CategoryId);
        var menu = category is null ? null : UnitOfWork.Menus.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == category.MenuId);
        if (menu is null)
        {
            throw new NotFoundException($"Menu '{menuItem.CategoryId}' was not found.");
        }

        EnsureRestaurantAccess(menu.RestaurantId);

        entity.Name = dto.Name;
        entity.NameAr = dto.NameAr;
        entity.IsRequired = dto.IsRequired;
        entity.MinSelections = dto.MinSelections;
        entity.MaxSelections = dto.MaxSelections;
        entity.SelectionType = dto.SelectionType;

        Repository.Update(entity);
        await UnitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public override async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = Repository.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == id);
        if (entity is null)
        {
            throw new NotFoundException($"{nameof(ItemOption)} '{id}' was not found.");
        }

        var menuItem = UnitOfWork.MenuItems.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == entity.MenuItemId);
        if (menuItem is null)
        {
            throw new NotFoundException($"Menu item '{entity.MenuItemId}' was not found.");
        }

        var category = UnitOfWork.Categories.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == menuItem.CategoryId);
        var menu = category is null ? null : UnitOfWork.Menus.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == category.MenuId);
        if (menu is null)
        {
            throw new NotFoundException($"Menu '{menuItem.CategoryId}' was not found.");
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
