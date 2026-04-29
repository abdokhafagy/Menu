using AutoMapper;

using Menu.Application.Common.Exceptions;
using Menu.Application.Common.Models;
using Menu.Application.DTOs.Menu;
using Menu.Application.Interfaces;
using Menu.Domain.Interfaces;

namespace Menu.Application.Services;

public class MenuService : CrudServiceBase<Menu.Domain.Entities.Menu, MenuDto, CreateMenuDto, UpdateMenuDto>, IMenuService
{
    private readonly ITenantContext _tenantContext;

    public MenuService(IUnitOfWork unitOfWork, IMapper mapper, ITenantContext tenantContext)
        : base(unitOfWork, mapper, unitOfWork.Menus)
    {
        _tenantContext = tenantContext;
    }

    public override Task<PaginatedResult<MenuDto>> GetAllAsync(QueryParameters parameters, CancellationToken ct = default)
    {
        var query =
            from menu in Repository.Query()
            join restaurant in UnitOfWork.Restaurants.Query() on menu.RestaurantId equals restaurant.Id
            select new MenuDto(menu.Id, menu.Name, menu.IsActive, menu.RestaurantId, menu.CreatedAt, restaurant.Name);

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var search = parameters.SearchTerm.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Name.ToLower().Contains(search) ||
                (x.RestaurantName ?? string.Empty).ToLower().Contains(search));
        }

        var totalCount = query.Count();
        var data = query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        return Task.FromResult(new PaginatedResult<MenuDto>
        {
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount,
            Data = data
        });
    }

    public override Task<MenuDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = Repository.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == id);
        if (entity is null)
        {
            throw new NotFoundException($"{nameof(Menu.Domain.Entities.Menu)} '{id}' was not found.");
        }

        EnsureRestaurantAccess(entity.RestaurantId);

        var dto = (
            from menu in Repository.Query()
            join restaurant in UnitOfWork.Restaurants.Query() on menu.RestaurantId equals restaurant.Id
            where menu.Id == id
            select new MenuDto(menu.Id, menu.Name, menu.IsActive, menu.RestaurantId, menu.CreatedAt, restaurant.Name))
            .FirstOrDefault();

        if (dto is null)
        {
            throw new NotFoundException($"{nameof(Menu.Domain.Entities.Menu)} '{id}' was not found.");
        }

        return Task.FromResult(dto);
    }

    public override async Task<MenuDto> CreateAsync(CreateMenuDto dto, CancellationToken ct = default)
    {
        var restaurantId = _tenantContext.RequiresRestaurantScope ? _tenantContext.GetRequiredRestaurantId() : dto.RestaurantId;
        if (restaurantId == Guid.Empty)
        {
            throw new BadRequestException("Restaurant is required.");
        }

        var restaurantExists = await UnitOfWork.Restaurants.ExistsAsync(x => x.Id == restaurantId, ct);
        if (!restaurantExists)
        {
            throw new NotFoundException($"Restaurant '{restaurantId}' was not found.");
        }

        var entity = new Menu.Domain.Entities.Menu
        {
            Name = dto.Name,
            IsActive = dto.IsActive,
            RestaurantId = restaurantId
        };

        await Repository.AddAsync(entity, ct);
        await UnitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct);
    }

    public override async Task<MenuDto> UpdateAsync(Guid id, UpdateMenuDto dto, CancellationToken ct = default)
    {
        var entity = Repository.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == id);
        if (entity is null)
        {
            throw new NotFoundException($"{nameof(Menu.Domain.Entities.Menu)} '{id}' was not found.");
        }

        EnsureRestaurantAccess(entity.RestaurantId);

        entity.Name = dto.Name;
        entity.IsActive = dto.IsActive;

        Repository.Update(entity);
        await UnitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public override async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = Repository.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == id);
        if (entity is null)
        {
            throw new NotFoundException($"{nameof(Menu.Domain.Entities.Menu)} '{id}' was not found.");
        }

        EnsureRestaurantAccess(entity.RestaurantId);

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
