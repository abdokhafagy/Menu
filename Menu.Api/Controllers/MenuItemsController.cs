using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Menu.Application.Common.Models;
using Menu.Application.DTOs.MenuItem;
using Menu.Application.Interfaces;
using Menu.Domain.Authorization;

namespace Menu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.Admin},{RoleNames.Manager}")]
public class MenuItemsController : ControllerBase
{
    private readonly IMenuItemService _service;

    public MenuItemsController(IMenuItemService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = PermissionNames.MenuItemsView)]
    public async Task<ActionResult<ApiResponse<PaginatedResult<MenuItemDto>>>> GetAll([FromQuery] QueryParameters query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return Ok(ApiResponse<PaginatedResult<MenuItemDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionNames.MenuItemsView)]
    public async Task<ActionResult<ApiResponse<MenuItemDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<MenuItemDto>.SuccessResponse(result));
    }

    [HttpGet("/api/categories/{categoryId:guid}/items")]
    [Authorize(Policy = PermissionNames.MenuItemsView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MenuItemDto>>>> GetByCategory(Guid categoryId, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(new QueryParameters { PageNumber = 1, PageSize = 100 }, ct);
        var filtered = result.Data.Where(x => x.CategoryId == categoryId).OrderBy(x => x.DisplayOrder).ToList();
        return Ok(ApiResponse<IReadOnlyList<MenuItemDto>>.SuccessResponse(filtered));
    }

    [HttpPost]
    [Authorize(Policy = PermissionNames.MenuItemsCreate)]
    public async Task<ActionResult<ApiResponse<MenuItemDto>>> Create([FromBody] CreateMenuItemDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return Ok(ApiResponse<MenuItemDto>.SuccessResponse(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionNames.MenuItemsUpdate)]
    public async Task<ActionResult<ApiResponse<MenuItemDto>>> Update(Guid id, [FromBody] UpdateMenuItemDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return Ok(ApiResponse<MenuItemDto>.SuccessResponse(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionNames.MenuItemsDelete)]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Ok(ApiResponse<string>.SuccessResponse("Deleted"));
    }
}
