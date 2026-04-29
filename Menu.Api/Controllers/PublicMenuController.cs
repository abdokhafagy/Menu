using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Menu.Application.Common.Models;
using Menu.Application.DTOs.Public;
using Menu.Application.Interfaces;

namespace Menu.Api.Controllers;

[ApiController]
[Route("api/public")]
[AllowAnonymous]
public class PublicMenuController : ControllerBase
{
    private readonly IPublicMenuService _service;

    public PublicMenuController(IPublicMenuService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all active restaurants. If only one exists, the client can auto-redirect.
    /// </summary>
    [HttpGet("restaurants")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    public async Task<ActionResult<ApiResponse<List<PublicRestaurantDto>>>> GetRestaurants(CancellationToken ct)
    {
        var result = await _service.GetAllRestaurantsAsync(ct);
        return Ok(ApiResponse<List<PublicRestaurantDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get restaurant by URL-friendly slug (e.g., "ch-culinary").
    /// </summary>
    [HttpGet("restaurants/by-slug/{slug}")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    public async Task<ActionResult<ApiResponse<PublicRestaurantDto>>> GetBySlug(string slug, CancellationToken ct)
    {
        var result = await _service.GetRestaurantBySlugAsync(slug, ct);
        return Ok(ApiResponse<PublicRestaurantDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Get restaurant by ID.
    /// </summary>
    [HttpGet("restaurants/{id:guid}")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    public async Task<ActionResult<ApiResponse<PublicRestaurantDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetRestaurantByIdAsync(id, ct);
        return Ok(ApiResponse<PublicRestaurantDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Get all active menus for a restaurant.
    /// </summary>
    [HttpGet("restaurants/{restaurantId:guid}/menus")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    public async Task<ActionResult<ApiResponse<List<PublicMenuDto>>>> GetMenus(Guid restaurantId, CancellationToken ct)
    {
        var result = await _service.GetMenusByRestaurantAsync(restaurantId, ct);
        return Ok(ApiResponse<List<PublicMenuDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get the lightweight public menu for a restaurant.
    /// Returns categories and basic menu cards only; item details are fetched on demand.
    /// </summary>
    [HttpGet("restaurants/{restaurantId:guid}/menu")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "menuId" })]
    public async Task<ActionResult<ApiResponse<PublicMenuSummaryDto>>> GetRestaurantMenu(
        Guid restaurantId,
        [FromQuery] Guid? menuId,
        CancellationToken ct)
    {
        var result = await _service.GetRestaurantMenuAsync(restaurantId, menuId, ct);
        return Ok(ApiResponse<PublicMenuSummaryDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Legacy full-tree endpoint. Prefer the lightweight restaurant menu endpoint for the public page.
    /// </summary>
    [HttpGet("menus/{menuId:guid}/full")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "includeOptions" })]
    public async Task<ActionResult<ApiResponse<PublicMenuFullDto>>> GetFullMenu(
        Guid menuId,
        [FromQuery] bool includeOptions = false,
        CancellationToken ct = default)
    {
        var result = await _service.GetFullMenuAsync(menuId, includeOptions, ct);
        return Ok(ApiResponse<PublicMenuFullDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Get item details with all options and values.
    /// </summary>
    [HttpGet("menu-items/{itemId:guid}")]
    [HttpGet("items/{itemId:guid}")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    public async Task<ActionResult<ApiResponse<PublicMenuItemDetailDto>>> GetItemDetail(Guid itemId, CancellationToken ct)
    {
        var result = await _service.GetItemDetailAsync(itemId, ct);
        return Ok(ApiResponse<PublicMenuItemDetailDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Search menu items by name (EN + AR).
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<List<PublicMenuItemDto>>>> Search(
        [FromQuery] string q,
        [FromQuery] Guid? restaurantId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
        {
            return Ok(ApiResponse<List<PublicMenuItemDto>>.SuccessResponse(new List<PublicMenuItemDto>()));
        }

        var result = await _service.SearchItemsAsync(q, restaurantId, ct);
        return Ok(ApiResponse<List<PublicMenuItemDto>>.SuccessResponse(result));
    }
}
