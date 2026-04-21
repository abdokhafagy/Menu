using Menu.UI.Models;

namespace Menu.UI.Services;

public abstract class CrudServiceBase<TDto, TCreate, TUpdate>
{
    private readonly IApiService _api;
    private readonly string _endpoint;

    protected CrudServiceBase(IApiService api, string endpoint)
    {
        _api = api;
        _endpoint = endpoint;
    }

    public Task<PaginatedResult<TDto>?> GetAllAsync(QueryParameters query, CancellationToken ct = default)
        => _api.GetPagedAsync<TDto>(_endpoint, query, ct);

    public Task<TDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _api.GetAsync<TDto>($"{_endpoint}/{id}", null, ct);

    public Task<TDto?> CreateAsync(TCreate dto, CancellationToken ct = default)
        => _api.PostAsync<TDto>(_endpoint, dto, ct);

    public Task<TDto?> UpdateAsync(Guid id, TUpdate dto, CancellationToken ct = default)
        => _api.PutAsync<TDto>($"{_endpoint}/{id}", dto, ct);

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        => _api.DeleteAsync($"{_endpoint}/{id}", ct);
}
