using AutoMapper;

using Menu.Application.Common.Exceptions;
using Menu.Application.Common.Models;
using Menu.Application.DTOs.OptionValue;
using Menu.Application.Interfaces;
using Menu.Domain.Entities;
using Menu.Domain.Interfaces;

namespace Menu.Application.Services;

public class OptionValueService : CrudServiceBase<OptionValue, OptionValueDto, CreateOptionValueDto, UpdateOptionValueDto>, IOptionValueService
{
    private readonly ITenantContext _tenantContext;

    public OptionValueService(IUnitOfWork unitOfWork, IMapper mapper, ITenantContext tenantContext)
        : base(unitOfWork, mapper, unitOfWork.OptionValues)
    {
        _tenantContext = tenantContext;
    }

    public override Task<PaginatedResult<OptionValueDto>> GetAllAsync(QueryParameters parameters, CancellationToken ct = default)
    {
        var query =
            from value in Repository.Query()
            join option in UnitOfWork.ItemOptions.Query() on value.ItemOptionId equals option.Id
            select new OptionValueDto(
                value.Id,
                value.Value,
                value.ValueAr,
                value.PriceModifier,
                value.IsDefault,
                value.DisplayOrder,
                value.ItemOptionId,
                value.CreatedAt,
                option.Name);

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var search = parameters.SearchTerm.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Value.ToLower().Contains(search) ||
                (x.ValueAr ?? string.Empty).ToLower().Contains(search) ||
                (x.ItemOptionName ?? string.Empty).ToLower().Contains(search));
        }

        var totalCount = query.Count();
        var data = query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        return Task.FromResult(new PaginatedResult<OptionValueDto>
        {
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount,
            Data = data
        });
    }

    public override Task<OptionValueDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = (
            from value in Repository.Query(ignoreQueryFilters: true)
            join option in UnitOfWork.ItemOptions.Query(ignoreQueryFilters: true) on value.ItemOptionId equals option.Id
            join menuItem in UnitOfWork.MenuItems.Query(ignoreQueryFilters: true) on option.MenuItemId equals menuItem.Id
            join category in UnitOfWork.Categories.Query(ignoreQueryFilters: true) on menuItem.CategoryId equals category.Id
            join menu in UnitOfWork.Menus.Query(ignoreQueryFilters: true) on category.MenuId equals menu.Id
            where value.Id == id
            select new { value, menu.RestaurantId, option.Name })
            .FirstOrDefault();

        if (entity is null)
        {
            throw new NotFoundException($"{nameof(OptionValue)} '{id}' was not found.");
        }

        EnsureRestaurantAccess(entity.RestaurantId);

        var dto = (
            from value in Repository.Query()
            join option in UnitOfWork.ItemOptions.Query() on value.ItemOptionId equals option.Id
            where value.Id == id
            select new OptionValueDto(
                value.Id,
                value.Value,
                value.ValueAr,
                value.PriceModifier,
                value.IsDefault,
                value.DisplayOrder,
                value.ItemOptionId,
                value.CreatedAt,
                option.Name))
            .FirstOrDefault();

        if (dto is null)
        {
            throw new NotFoundException($"{nameof(OptionValue)} '{id}' was not found.");
        }

        return Task.FromResult(dto);
    }

    public override async Task<OptionValueDto> CreateAsync(CreateOptionValueDto dto, CancellationToken ct = default)
    {
        var option = UnitOfWork.ItemOptions.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == dto.ItemOptionId);
        if (option is null)
        {
            throw new NotFoundException($"Item option '{dto.ItemOptionId}' was not found.");
        }

        var menuItem = UnitOfWork.MenuItems.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == option.MenuItemId);
        var category = menuItem is null ? null : UnitOfWork.Categories.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == menuItem.CategoryId);
        var menu = category is null ? null : UnitOfWork.Menus.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == category.MenuId);
        if (menu is null)
        {
            throw new NotFoundException($"Menu '{option.MenuItemId}' was not found.");
        }

        EnsureRestaurantAccess(menu.RestaurantId);

        var entity = new OptionValue
        {
            Value = dto.Value,
            ValueAr = dto.ValueAr,
            PriceModifier = dto.PriceModifier,
            IsDefault = dto.IsDefault,
            DisplayOrder = dto.DisplayOrder,
            ItemOptionId = dto.ItemOptionId
        };

        await Repository.AddAsync(entity, ct);
        await UnitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct);
    }

    public override async Task<OptionValueDto> UpdateAsync(Guid id, UpdateOptionValueDto dto, CancellationToken ct = default)
    {
        var entity = Repository.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == id);
        if (entity is null)
        {
            throw new NotFoundException($"{nameof(OptionValue)} '{id}' was not found.");
        }

        var option = UnitOfWork.ItemOptions.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == entity.ItemOptionId);
        if (option is null)
        {
            throw new NotFoundException($"Item option '{entity.ItemOptionId}' was not found.");
        }

        var menuItem = UnitOfWork.MenuItems.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == option.MenuItemId);
        var category = menuItem is null ? null : UnitOfWork.Categories.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == menuItem.CategoryId);
        var menu = category is null ? null : UnitOfWork.Menus.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == category.MenuId);
        if (menu is null)
        {
            throw new NotFoundException($"Menu '{option.MenuItemId}' was not found.");
        }

        EnsureRestaurantAccess(menu.RestaurantId);

        entity.Value = dto.Value;
        entity.ValueAr = dto.ValueAr;
        entity.PriceModifier = dto.PriceModifier;
        entity.IsDefault = dto.IsDefault;
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
            throw new NotFoundException($"{nameof(OptionValue)} '{id}' was not found.");
        }

        var option = UnitOfWork.ItemOptions.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == entity.ItemOptionId);
        if (option is null)
        {
            throw new NotFoundException($"Item option '{entity.ItemOptionId}' was not found.");
        }

        var menuItem = UnitOfWork.MenuItems.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == option.MenuItemId);
        var category = menuItem is null ? null : UnitOfWork.Categories.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == menuItem.CategoryId);
        var menu = category is null ? null : UnitOfWork.Menus.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == category.MenuId);
        if (menu is null)
        {
            throw new NotFoundException($"Menu '{option.MenuItemId}' was not found.");
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
