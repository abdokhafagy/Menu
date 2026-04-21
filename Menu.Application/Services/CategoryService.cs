using AutoMapper;

using Menu.Application.Common.Exceptions;
using Menu.Application.Common.Models;
using Menu.Application.DTOs.Category;
using Menu.Application.Interfaces;
using Menu.Domain.Interfaces;

namespace Menu.Application.Services;

public class CategoryService : CrudServiceBase<Menu.Domain.Entities.Category, CategoryDto, CreateCategoryDto, UpdateCategoryDto>, ICategoryService
{
    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper, unitOfWork.Categories)
    {
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
}
