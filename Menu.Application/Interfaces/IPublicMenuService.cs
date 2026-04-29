using Menu.Application.DTOs.Public;

namespace Menu.Application.Interfaces;

public interface IPublicMenuService
{
    Task<List<PublicRestaurantDto>> GetAllRestaurantsAsync(CancellationToken ct = default);
    Task<PublicRestaurantDto> GetRestaurantBySlugAsync(string slug, CancellationToken ct = default);
    Task<PublicRestaurantDto> GetRestaurantByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<PublicMenuDto>> GetMenusByRestaurantAsync(Guid restaurantId, CancellationToken ct = default);
    Task<PublicMenuSummaryDto> GetRestaurantMenuAsync(Guid restaurantId, Guid? menuId = null, CancellationToken ct = default);
    Task<PublicMenuFullDto> GetFullMenuAsync(Guid menuId, bool includeOptions = false, CancellationToken ct = default);
    Task<PublicMenuItemDetailDto> GetItemDetailAsync(Guid itemId, CancellationToken ct = default);
    Task<List<PublicMenuItemDto>> SearchItemsAsync(string query, Guid? restaurantId, CancellationToken ct = default);
}
