using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Menu.Application.Common.Models;
using Menu.Application.DTOs.ItemOption;
using Menu.Application.Interfaces;
using Menu.Domain.Authorization;

namespace Menu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.Admin},{RoleNames.Manager}")]
public class ItemOptionsController : ControllerBase
{
    private readonly IItemOptionService _service;

    public ItemOptionsController(IItemOptionService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = PermissionNames.ItemOptionsView)]
    public async Task<ActionResult<ApiResponse<PaginatedResult<ItemOptionDto>>>> GetAll([FromQuery] QueryParameters query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return Ok(ApiResponse<PaginatedResult<ItemOptionDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionNames.ItemOptionsView)]
    public async Task<ActionResult<ApiResponse<ItemOptionDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<ItemOptionDto>.SuccessResponse(result));
    }

    [HttpGet("/api/menu-items/{itemId:guid}/options")]
    [Authorize(Policy = PermissionNames.ItemOptionsView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ItemOptionDto>>>> GetByMenuItem(Guid itemId, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(new QueryParameters { PageNumber = 1, PageSize = 100 }, ct);
        var filtered = result.Data.Where(x => x.MenuItemId == itemId).ToList();
        return Ok(ApiResponse<IReadOnlyList<ItemOptionDto>>.SuccessResponse(filtered));
    }

    [HttpPost]
    [Authorize(Policy = PermissionNames.ItemOptionsCreate)]
    public async Task<ActionResult<ApiResponse<ItemOptionDto>>> Create([FromBody] CreateItemOptionDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return Ok(ApiResponse<ItemOptionDto>.SuccessResponse(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionNames.ItemOptionsUpdate)]
    public async Task<ActionResult<ApiResponse<ItemOptionDto>>> Update(Guid id, [FromBody] UpdateItemOptionDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return Ok(ApiResponse<ItemOptionDto>.SuccessResponse(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionNames.ItemOptionsDelete)]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Ok(ApiResponse<string>.SuccessResponse("Deleted"));
    }
}
