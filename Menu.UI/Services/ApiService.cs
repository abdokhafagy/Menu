using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using Menu.UI.Models;

namespace Menu.UI.Services;

public sealed class ApiService : IApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<T?> GetAsync<T>(string url, QueryParameters? query = null, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync(AddQueryString(url, query), ct);
        return await ReadDataAsync<T>(response, ct);
    }

    public async Task<PaginatedResult<T>?> GetPagedAsync<T>(string url, QueryParameters query, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync(AddQueryString(url, query), ct);
        return await ReadDataAsync<PaginatedResult<T>>(response, ct);
    }

    public async Task<T?> PostAsync<T>(string url, object? data = null, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync(url, data, ct);
        return await ReadDataAsync<T>(response, ct);
    }

    public async Task<T?> PutAsync<T>(string url, object? data = null, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsJsonAsync(url, data, ct);
        return await ReadDataAsync<T>(response, ct);
    }

    public async Task<bool> DeleteAsync(string url, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync(url, ct);
        await EnsureSuccessAsync(response, ct);
        return true;
    }

    public async Task<string?> PostStringAsync(string url, object? data = null, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync(url, data, ct);
        return await ReadDataAsync<string>(response, ct);
    }

    private static async Task<T?> ReadDataAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        await EnsureSuccessAsync(response, ct);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions, ct);
        if (payload is null)
        {
            throw new InvalidOperationException("API response body was empty.");
        }

        if (!payload.Success)
        {
            var message = payload.Message ?? "The API returned a failed response.";
            throw new InvalidOperationException(message);
        }

        return payload.Data;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(body))
        {
            response.EnsureSuccessStatusCode();
        }

        try
        {
            var error = JsonSerializer.Deserialize<ApiResponse<object>>(body, JsonOptions);
            var message = error?.Message;
            if (!string.IsNullOrWhiteSpace(message))
            {
                throw new InvalidOperationException(message);
            }
        }
        catch (JsonException)
        {
            // Ignore payload parse errors and throw status code error below.
        }

        response.EnsureSuccessStatusCode();
    }

    private static string AddQueryString(string url, QueryParameters? query)
    {
        if (query is null)
        {
            return url;
        }

        var parts = new List<string>
        {
            $"pageNumber={query.PageNumber}",
            $"pageSize={query.PageSize}"
        };

        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            parts.Add($"sortBy={Uri.EscapeDataString(query.SortBy)}");
        }

        parts.Add($"sortDescending={query.SortDescending.ToString().ToLowerInvariant()}");

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            parts.Add($"searchTerm={Uri.EscapeDataString(query.SearchTerm)}");
        }

        if (query.Filters is not null)
        {
            foreach (var filter in query.Filters)
            {
                parts.Add($"filters[{Uri.EscapeDataString(filter.Key)}]={Uri.EscapeDataString(filter.Value)}");
            }
        }

        var separator = url.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return new StringBuilder(url).Append(separator).Append(string.Join("&", parts)).ToString();
    }
}
