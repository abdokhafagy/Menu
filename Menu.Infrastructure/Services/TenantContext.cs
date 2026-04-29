using System.Security.Claims;

using Menu.Application.Common.Exceptions;
using Menu.Application.Interfaces;
using Menu.Domain.Authorization;
using Microsoft.AspNetCore.Http;

namespace Menu.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated => GetUser()?.Identity?.IsAuthenticated == true;

    public bool IsSuperAdmin => GetUser()?.IsInRole(RoleNames.SuperAdmin) == true;

    public bool RequiresRestaurantScope => IsAuthenticated && !IsSuperAdmin;

    public Guid? RestaurantId => TryGetRestaurantId();

    public bool CanAccessRestaurant(Guid restaurantId)
    {
        return !RequiresRestaurantScope || (RestaurantId.HasValue && RestaurantId.Value == restaurantId);
    }

    public Guid GetRequiredRestaurantId()
    {
        if (!RequiresRestaurantScope)
        {
            throw new UnauthorizedException("Restaurant context is not required for the current user.");
        }

        if (!RestaurantId.HasValue)
        {
            throw new UnauthorizedException("Restaurant identifier is missing from the current token.");
        }

        return RestaurantId.Value;
    }

    private ClaimsPrincipal? GetUser()
    {
        return _httpContextAccessor.HttpContext?.User;
    }

    private Guid? TryGetRestaurantId()
    {
        var rawRestaurantId = GetUser()?.FindFirst(JwtClaimTypes.RestaurantId)?.Value;
        return Guid.TryParse(rawRestaurantId, out var restaurantId) ? restaurantId : null;
    }
}