using AutoMapper;

using Menu.Application.Common.Exceptions;
using Menu.Application.Common.Models;
using Menu.Application.DTOs.Menu;
using Menu.Application.Interfaces;
using Menu.Domain.Interfaces;

namespace Menu.Application.Services;

public class MenuService : CrudServiceBase<Menu.Domain.Entities.Menu, MenuDto, CreateMenuDto, UpdateMenuDto>, IMenuService
{
    public MenuService(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper, unitOfWork.Menus)
    {
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
}
