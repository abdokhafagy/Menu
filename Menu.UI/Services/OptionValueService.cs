using Menu.UI.Models.OptionValue;

namespace Menu.UI.Services;

public sealed class OptionValueService : CrudServiceBase<OptionValueDto, CreateOptionValueRequest, UpdateOptionValueRequest>
{
    private readonly IApiService _api;

    public OptionValueService(IApiService api) : base(api, "api/optionvalues")
    {
        _api = api;
    }

    public async Task<IReadOnlyList<OptionValueDto>> GetByOptionAsync(Guid optionId, CancellationToken ct = default)
    {
        return await _api.GetAsync<IReadOnlyList<OptionValueDto>>($"api/item-options/{optionId}/values", null, ct) ?? Array.Empty<OptionValueDto>();
    }
}
