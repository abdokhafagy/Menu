using Menu.UI.Models.Restaurant;

namespace Menu.UI.Services;

public sealed class RestaurantService : CrudServiceBase<RestaurantDto, CreateRestaurantRequest, UpdateRestaurantRequest>
{
    public RestaurantService(IApiService api) : base(api, "api/restaurants")
    {
    }
}
