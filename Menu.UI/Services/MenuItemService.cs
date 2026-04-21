using Menu.UI.Models.MenuItem;

namespace Menu.UI.Services;

public sealed class MenuItemService : CrudServiceBase<MenuItemDto, CreateMenuItemRequest, UpdateMenuItemRequest>
{
    private readonly IApiService _api;

    public MenuItemService(IApiService api) : base(api, "api/menuitems")
    {
        _api = api;
    }

    public async Task<IReadOnlyList<MenuItemDto>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await _api.GetAsync<IReadOnlyList<MenuItemDto>>($"api/categories/{categoryId}/items", null, ct) ?? Array.Empty<MenuItemDto>();
    }
}
