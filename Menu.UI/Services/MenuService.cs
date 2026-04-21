using Menu.UI.Models.Menu;

namespace Menu.UI.Services;

public sealed class MenuService : CrudServiceBase<MenuDto, CreateMenuRequest, UpdateMenuRequest>
{
    private readonly IApiService _api;

    public MenuService(IApiService api) : base(api, "api/menus")
    {
        _api = api;
    }

    public async Task<IReadOnlyList<MenuDto>> GetByRestaurantAsync(Guid restaurantId, CancellationToken ct = default)
    {
        return await _api.GetAsync<IReadOnlyList<MenuDto>>($"api/restaurants/{restaurantId}/menus", null, ct) ?? Array.Empty<MenuDto>();
    }
}
