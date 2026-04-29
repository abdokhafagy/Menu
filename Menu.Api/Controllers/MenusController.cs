using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Menu.Application.Common.Models;
using Menu.Application.DTOs.Menu;
using Menu.Application.Interfaces;
using Menu.Domain.Authorization;

namespace Menu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.Admin},{RoleNames.Manager}")]
public class MenusController : ControllerBase
{
    private readonly IMenuService _service;

    public MenusController(IMenuService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = PermissionNames.MenusView)]
    public async Task<ActionResult<ApiResponse<PaginatedResult<MenuDto>>>> GetAll([FromQuery] QueryParameters query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return Ok(ApiResponse<PaginatedResult<MenuDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionNames.MenusView)]
    public async Task<ActionResult<ApiResponse<MenuDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<MenuDto>.SuccessResponse(result));
    }

    [HttpGet("/api/restaurants/{restaurantId:guid}/menus")]
    [Authorize(Policy = PermissionNames.MenusView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MenuDto>>>> GetByRestaurant(Guid restaurantId, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(new QueryParameters { PageNumber = 1, PageSize = 50 }, ct);
        var filtered = result.Data.Where(x => x.RestaurantId == restaurantId).ToList();
        return Ok(ApiResponse<IReadOnlyList<MenuDto>>.SuccessResponse(filtered));
    }

    [HttpPost]
    [Authorize(Policy = PermissionNames.MenusCreate)]
    public async Task<ActionResult<ApiResponse<MenuDto>>> Create([FromBody] CreateMenuDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return Ok(ApiResponse<MenuDto>.SuccessResponse(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionNames.MenusUpdate)]
    public async Task<ActionResult<ApiResponse<MenuDto>>> Update(Guid id, [FromBody] UpdateMenuDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return Ok(ApiResponse<MenuDto>.SuccessResponse(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionNames.MenusDelete)]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Ok(ApiResponse<string>.SuccessResponse("Deleted"));
    }
}
