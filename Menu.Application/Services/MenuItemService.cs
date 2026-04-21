using AutoMapper;

using Menu.Application.Common.Exceptions;
using Menu.Application.Common.Models;
using Menu.Application.DTOs.MenuItem;
using Menu.Application.Interfaces;
using Menu.Domain.Interfaces;

namespace Menu.Application.Services;

public class MenuItemService : CrudServiceBase<Menu.Domain.Entities.MenuItem, MenuItemDto, CreateMenuItemDto, UpdateMenuItemDto>, IMenuItemService
{
    public MenuItemService(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper, unitOfWork.MenuItems)
    {
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
            throw new NotFoundException($"{nameof(Menu.Domain.Entities.MenuItem)} '{id}' was not found.");
        }

        return Task.FromResult(dto);
    }
}
