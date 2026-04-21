using Blazored.LocalStorage;

namespace Menu.UI.Auth;

public sealed class TokenService
{
    private const string AccessTokenKey = "accessToken";
    private const string RefreshTokenKey = "refreshToken";

    private readonly ILocalStorageService _localStorage;

    public TokenService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public Task SaveTokensAsync(string accessToken, string refreshToken)
    {
        return Task.WhenAll(
            _localStorage.SetItemAsync(AccessTokenKey, accessToken).AsTask(),
            _localStorage.SetItemAsync(RefreshTokenKey, refreshToken).AsTask());
    }

    public Task<string?> GetAccessTokenAsync()
    {
        return _localStorage.GetItemAsync<string?>(AccessTokenKey).AsTask();
    }

    public Task<string?> GetRefreshTokenAsync()
    {
        return _localStorage.GetItemAsync<string?>(RefreshTokenKey).AsTask();
    }

    public Task ClearTokensAsync()
    {
        return Task.WhenAll(
            _localStorage.RemoveItemAsync(AccessTokenKey).AsTask(),
            _localStorage.RemoveItemAsync(RefreshTokenKey).AsTask());
    }
}
