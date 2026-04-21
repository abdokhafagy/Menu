using Menu.Application.Common.Models;

namespace Menu.Application.Interfaces;

public interface ICrudService<TDto, TCreateDto, TUpdateDto>
{
    Task<TDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PaginatedResult<TDto>> GetAllAsync(QueryParameters parameters, CancellationToken ct = default);
    Task<TDto> CreateAsync(TCreateDto dto, CancellationToken ct = default);
    Task<TDto> UpdateAsync(Guid id, TUpdateDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
