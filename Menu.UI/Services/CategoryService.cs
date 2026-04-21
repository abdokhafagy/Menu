using Menu.UI.Models.Category;

namespace Menu.UI.Services;

public sealed class CategoryService : CrudServiceBase<CategoryDto, CreateCategoryRequest, UpdateCategoryRequest>
{
    private readonly IApiService _api;

    public CategoryService(IApiService api) : base(api, "api/categories")
    {
        _api = api;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetByMenuAsync(Guid menuId, CancellationToken ct = default)
    {
        return await _api.GetAsync<IReadOnlyList<CategoryDto>>($"api/menus/{menuId}/categories", null, ct) ?? Array.Empty<CategoryDto>();
    }
}
