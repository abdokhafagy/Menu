using Menu.UI.Models.User;

namespace Menu.UI.Services;

public sealed class UserService : CrudServiceBase<UserDto, CreateUserRequest, UpdateUserRequest>
{
    private readonly IApiService _api;

    public UserService(IApiService api) : base(api, "api/users")
    {
        _api = api;
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync(Guid userId, CancellationToken ct = default)
    {
        return await _api.GetAsync<IReadOnlyList<string>>($"api/users/{userId}/roles", null, ct) ?? Array.Empty<string>();
    }

    public Task<string?> AssignRolesAsync(Guid userId, AssignRolesRequest request, CancellationToken ct = default)
    {
        return _api.PostStringAsync($"api/users/{userId}/roles", request, ct);
    }
}
