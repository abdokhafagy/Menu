using Menu.UI.Models.ItemOption;

namespace Menu.UI.Services;

public sealed class ItemOptionService : CrudServiceBase<ItemOptionDto, CreateItemOptionRequest, UpdateItemOptionRequest>
{
    private readonly IApiService _api;

    public ItemOptionService(IApiService api) : base(api, "api/itemoptions")
    {
        _api = api;
    }

    public async Task<IReadOnlyList<ItemOptionDto>> GetByMenuItemAsync(Guid itemId, CancellationToken ct = default)
    {
        return await _api.GetAsync<IReadOnlyList<ItemOptionDto>>($"api/menu-items/{itemId}/options", null, ct) ?? Array.Empty<ItemOptionDto>();
    }
}
