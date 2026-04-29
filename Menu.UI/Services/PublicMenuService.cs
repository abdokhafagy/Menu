using System.Net.Http.Json;
using System.Text.Json;

using Menu.UI.Models;
using Menu.UI.Models.Public;

namespace Menu.UI.Services;

public sealed class PublicMenuService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;

    public PublicMenuService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public Task<PublicRestaurantDto?> GetRestaurantByIdAsync(Guid restaurantId, CancellationToken ct = default)
        => GetPublicAsync<PublicRestaurantDto>($"api/public/restaurants/{restaurantId}", ct);

    public async Task<IReadOnlyList<PublicMenuDto>> GetMenusByRestaurantAsync(Guid restaurantId, CancellationToken ct = default)
    {
        var menus = await GetPublicAsync<List<PublicMenuDto>>($"api/public/restaurants/{restaurantId}/menus", ct);
        return menus ?? new List<PublicMenuDto>();
    }

    public Task<PublicMenuSummaryDto?> GetRestaurantMenuAsync(Guid restaurantId, Guid? menuId = null, CancellationToken ct = default)
    {
        var url = menuId.HasValue
            ? $"api/public/restaurants/{restaurantId}/menu?menuId={menuId.Value}"
            : $"api/public/restaurants/{restaurantId}/menu";

        return GetPublicAsync<PublicMenuSummaryDto>(url, ct);
    }

    public Task<PublicMenuFullDto?> GetFullMenuAsync(Guid menuId, bool includeOptions = false, CancellationToken ct = default)
        => GetPublicAsync<PublicMenuFullDto>($"api/public/menus/{menuId}/full?includeOptions={includeOptions.ToString().ToLowerInvariant()}", ct);

    public Task<PublicMenuItemDetailDto?> GetItemDetailAsync(Guid itemId, CancellationToken ct = default)
        => GetPublicAsync<PublicMenuItemDetailDto>($"api/public/menu-items/{itemId}", ct);

    private async Task<T?> GetPublicAsync<T>(string url, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("MenuApiNoAuth");
        using var response = await client.GetAsync(url, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw ParseErrorOrThrow(response, body);
        }

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions, ct);
        if (payload is null)
        {
            throw new InvalidOperationException("API response body was empty.");
        }

        if (!payload.Success)
        {
            throw new InvalidOperationException(payload.Message ?? "The API returned a failed response.");
        }

        return payload.Data;
    }

    private static Exception ParseErrorOrThrow(HttpResponseMessage response, string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return new HttpRequestException($"Request failed with status code {(int)response.StatusCode}.");
        }

        try
        {
            var error = JsonSerializer.Deserialize<ApiResponse<object>>(body, JsonOptions);
            if (!string.IsNullOrWhiteSpace(error?.Message))
            {
                return new InvalidOperationException(error.Message);
            }
        }
        catch (JsonException)
        {
            // Ignore payload parse errors and return fallback below.
        }

        return new HttpRequestException($"Request failed with status code {(int)response.StatusCode}.");
    }
}
