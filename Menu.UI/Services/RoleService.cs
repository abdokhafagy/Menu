using Menu.UI.Models.Permission;
using Menu.UI.Models.Role;

namespace Menu.UI.Services;

public sealed class RoleService : CrudServiceBase<RoleDto, CreateRoleRequest, UpdateRoleRequest>
{
    private readonly IApiService _api;

    public RoleService(IApiService api) : base(api, "api/roles")
    {
        _api = api;
    }

    public Task<string?> AssignPermissionsAsync(Guid roleId, AssignPermissionsRequest request, CancellationToken ct = default)
    {
        return _api.PostStringAsync($"api/roles/{roleId}/permissions", request, ct);
    }
}
