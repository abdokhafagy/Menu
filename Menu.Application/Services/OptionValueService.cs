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
    public OptionValueService(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper, unitOfWork.OptionValues)
    {
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
}
