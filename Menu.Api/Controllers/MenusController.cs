using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Menu.Api.Filters;
using Menu.Application.Common.Models;
using Menu.Application.DTOs.Menu;
using Menu.Application.Interfaces;

namespace Menu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class MenusController : ControllerBase
{
    private readonly IMenuService _service;

    public MenusController(IMenuService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<MenuDto>>>> GetAll([FromQuery] QueryParameters query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return Ok(ApiResponse<PaginatedResult<MenuDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<MenuDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<MenuDto>.SuccessResponse(result));
    }

    [HttpGet("/api/restaurants/{restaurantId:guid}/menus")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MenuDto>>>> GetByRestaurant(Guid restaurantId, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(new QueryParameters { PageNumber = 1, PageSize = 50 }, ct);
        var filtered = result.Data.Where(x => x.RestaurantId == restaurantId).ToList();
        return Ok(ApiResponse<IReadOnlyList<MenuDto>>.SuccessResponse(filtered));
    }

    [HttpPost]
    [RequirePermission("menus.create")]
    public async Task<ActionResult<ApiResponse<MenuDto>>> Create([FromBody] CreateMenuDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return Ok(ApiResponse<MenuDto>.SuccessResponse(result));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("menus.update")]
    public async Task<ActionResult<ApiResponse<MenuDto>>> Update(Guid id, [FromBody] UpdateMenuDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return Ok(ApiResponse<MenuDto>.SuccessResponse(result));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("menus.delete")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Ok(ApiResponse<string>.SuccessResponse("Deleted"));
    }
}
