using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

using Menu.UI.Auth;
using Menu.UI.Models;
using Menu.UI.Models.Auth;
using Menu.UI.Models.Restaurant;

namespace Menu.UI.Services;

public sealed class AuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TokenService _tokenService;
    private readonly CustomAuthStateProvider _authStateProvider;

    public AuthService(
        IHttpClientFactory httpClientFactory,
        TokenService tokenService,
        CustomAuthStateProvider authStateProvider)
    {
        _httpClientFactory = httpClientFactory;
        _tokenService = tokenService;
        _authStateProvider = authStateProvider;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("MenuApiNoAuth");
        var response = await client.PostAsJsonAsync("api/auth/login", request, ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(cancellationToken: ct);
        if (payload?.Data is null)
        {
            return null;
        }

        await _tokenService.SaveTokensAsync(payload.Data.AccessToken, payload.Data.RefreshToken);
        _authStateProvider.NotifyUserAuthentication(payload.Data.AccessToken);
        return payload.Data;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("MenuApi");
        var response = await client.PostAsJsonAsync("api/auth/register", request, ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(cancellationToken: ct);
        return payload?.Data;
    }

    public async Task<AuthResponse?> RefreshAsync(CancellationToken ct = default)
    {
        var accessToken = await _tokenService.GetAccessTokenAsync();
        var refreshToken = await _tokenService.GetRefreshTokenAsync();
        if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var client = _httpClientFactory.CreateClient("MenuApiNoAuth");
        var response = await client.PostAsJsonAsync("api/auth/refresh", new RefreshTokenRequest(accessToken, refreshToken), ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(cancellationToken: ct);
        if (payload?.Data is null)
        {
            return null;
        }

        await _tokenService.SaveTokensAsync(payload.Data.AccessToken, payload.Data.RefreshToken);
        _authStateProvider.NotifyUserAuthentication(payload.Data.AccessToken);
        return payload.Data;
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("MenuApi");
        _ = await client.PostAsJsonAsync("api/auth/logout", new { }, ct);
        await _tokenService.ClearTokensAsync();
        _authStateProvider.NotifyUserLogout();
    }

    public async Task<PaginatedResult<SessionInfo>> GetSessionsAsync(QueryParameters query, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("MenuApi");
        var response = await client.GetAsync($"api/auth/sessions?pageNumber={query.PageNumber}&pageSize={query.PageSize}", ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(body))
        {
            return CreateEmptySessionsResult(query);
        }

        using var document = JsonDocument.Parse(body);
        if (document.RootElement.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
        {
            var items = JsonSerializer.Deserialize<IReadOnlyList<SessionInfo>>(dataElement.GetRawText(), JsonOptions) ?? Array.Empty<SessionInfo>();
            return CreateLegacySessionsResult(query, items);
        }

        var payload = JsonSerializer.Deserialize<ApiResponse<PaginatedResult<SessionInfo>>>(body, JsonOptions);
        return payload?.Data ?? CreateEmptySessionsResult(query);
    }

    public async Task RevokeSessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("MenuApi");
        var response = await client.PostAsJsonAsync($"api/auth/sessions/{sessionId}/revoke", new { }, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<RestaurantDto>> GetRestaurantsAsync(CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("MenuApi");
        var response = await client.GetAsync("api/restaurants?pageNumber=1&pageSize=100", ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResult<RestaurantDto>>>(cancellationToken: ct);
        return payload?.Data?.Data ?? Array.Empty<RestaurantDto>();
    }

    private static PaginatedResult<SessionInfo> CreateLegacySessionsResult(QueryParameters query, IReadOnlyList<SessionInfo> items)
    {
        var totalCount = items.Count;
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return new PaginatedResult<SessionInfo>
        {
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = query.PageNumber > 1,
            HasNextPage = query.PageNumber < totalPages,
            Data = items
        };
    }

    private static PaginatedResult<SessionInfo> CreateEmptySessionsResult(QueryParameters query)
    {
        return new PaginatedResult<SessionInfo>
        {
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            Data = Array.Empty<SessionInfo>()
        };
    }
}
