namespace Menu.UI.Services;

using Menu.UI.Models;

public interface IApiService
{
    Task<T?> GetAsync<T>(string url, QueryParameters? query = null, CancellationToken ct = default);
    Task<PaginatedResult<T>?> GetPagedAsync<T>(string url, QueryParameters query, CancellationToken ct = default);
    Task<T?> PostAsync<T>(string url, object? data = null, CancellationToken ct = default);
    Task<T?> PutAsync<T>(string url, object? data = null, CancellationToken ct = default);
    Task<bool> DeleteAsync(string url, CancellationToken ct = default);
    Task<string?> PostStringAsync(string url, object? data = null, CancellationToken ct = default);
}
