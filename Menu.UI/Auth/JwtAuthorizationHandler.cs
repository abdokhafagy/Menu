using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using Menu.UI.Models;
using Menu.UI.Models.Auth;

using Microsoft.AspNetCore.Components;

namespace Menu.UI.Auth;

public sealed class JwtAuthorizationHandler : DelegatingHandler
{
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    private readonly TokenService _tokenService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CustomAuthStateProvider _authStateProvider;
    private readonly NavigationManager _navigationManager;

    public JwtAuthorizationHandler(
        TokenService tokenService,
        IHttpClientFactory httpClientFactory,
        CustomAuthStateProvider authStateProvider,
        NavigationManager navigationManager)
    {
        _tokenService = tokenService;
        _httpClientFactory = httpClientFactory;
        _authStateProvider = authStateProvider;
        _navigationManager = navigationManager;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var currentAccessToken = await _tokenService.GetAccessTokenAsync();
        if (!string.IsNullOrWhiteSpace(currentAccessToken) && IsTokenExpired(currentAccessToken))
        {
            _ = await TryRefreshTokenAsync(cancellationToken);
        }

        await AttachTokenAsync(request);

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        var refreshed = await TryRefreshTokenAsync(cancellationToken);
        if (!refreshed)
        {
            return response;
        }

        var retryRequest = await CloneRequestAsync(request);
        await AttachTokenAsync(retryRequest);
        response.Dispose();
        return await base.SendAsync(retryRequest, cancellationToken);
    }

    private async Task AttachTokenAsync(HttpRequestMessage request)
    {
        var token = await _tokenService.GetAccessTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private async Task<bool> TryRefreshTokenAsync(CancellationToken cancellationToken)
    {
        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            var existingAccessToken = await _tokenService.GetAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(existingAccessToken) && !IsTokenExpired(existingAccessToken))
            {
                return true;
            }

            var accessToken = await _tokenService.GetAccessTokenAsync();
            var refreshToken = await _tokenService.GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
            {
                return false;
            }

            var refreshClient = _httpClientFactory.CreateClient("MenuApiNoAuth");
            using var refreshResponse = await refreshClient.PostAsJsonAsync("api/auth/refresh", new RefreshTokenRequest(accessToken, refreshToken), cancellationToken);
            if (!refreshResponse.IsSuccessStatusCode)
            {
                await LogoutAsync();
                return false;
            }

            var payload = await refreshResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, cancellationToken);

            if (payload is null || !payload.Success || payload.Data is null)
            {
                await LogoutAsync();
                return false;
            }

            await _tokenService.SaveTokensAsync(payload.Data.AccessToken, payload.Data.RefreshToken);
            _authStateProvider.NotifyUserAuthentication(payload.Data.AccessToken);
            return true;
        }
        catch
        {
            await LogoutAsync();
            return false;
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    private static bool IsTokenExpired(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.ValidTo <= DateTime.UtcNow;
        }
        catch
        {
            return true;
        }
    }

    private async Task LogoutAsync()
    {
        await _tokenService.ClearTokensAsync();
        _authStateProvider.NotifyUserLogout();
        _navigationManager.NavigateTo("/login", true);
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (request.Content is not null)
        {
            var ms = new MemoryStream();
            await request.Content.CopyToAsync(ms);
            ms.Position = 0;
            var copiedContent = new StreamContent(ms);

            foreach (var header in request.Content.Headers)
            {
                copiedContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            clone.Content = copiedContent;
        }

        foreach (var option in request.Options)
        {
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
        }

        return clone;
    }
}
