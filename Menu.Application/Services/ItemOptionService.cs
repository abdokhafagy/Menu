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
    public ItemOptionService(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper, unitOfWork.ItemOptions)
    {
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
}
