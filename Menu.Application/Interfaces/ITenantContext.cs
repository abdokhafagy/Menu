namespace Menu.Application.Interfaces;

public interface ITenantContext
{
    bool IsAuthenticated { get; }
    bool IsSuperAdmin { get; }
    bool RequiresRestaurantScope { get; }
    Guid? RestaurantId { get; }

    bool CanAccessRestaurant(Guid restaurantId);
    Guid GetRequiredRestaurantId();
}