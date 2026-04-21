using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Menu.Api.Filters;
using Menu.Application.Common.Models;
using Menu.Application.DTOs.ItemOption;
using Menu.Application.Interfaces;

namespace Menu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemOptionsController : ControllerBase
{
    private readonly IItemOptionService _service;

    public ItemOptionsController(IItemOptionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<ItemOptionDto>>>> GetAll([FromQuery] QueryParameters query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return Ok(ApiResponse<PaginatedResult<ItemOptionDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ItemOptionDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<ItemOptionDto>.SuccessResponse(result));
    }

    [HttpGet("/api/menu-items/{itemId:guid}/options")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ItemOptionDto>>>> GetByMenuItem(Guid itemId, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(new QueryParameters { PageNumber = 1, PageSize = 100 }, ct);
        var filtered = result.Data.Where(x => x.MenuItemId == itemId).ToList();
        return Ok(ApiResponse<IReadOnlyList<ItemOptionDto>>.SuccessResponse(filtered));
    }

    [HttpPost]
    [RequirePermission("itemoptions.create")]
    public async Task<ActionResult<ApiResponse<ItemOptionDto>>> Create([FromBody] CreateItemOptionDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return Ok(ApiResponse<ItemOptionDto>.SuccessResponse(result));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("itemoptions.update")]
    public async Task<ActionResult<ApiResponse<ItemOptionDto>>> Update(Guid id, [FromBody] UpdateItemOptionDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return Ok(ApiResponse<ItemOptionDto>.SuccessResponse(result));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("itemoptions.delete")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Ok(ApiResponse<string>.SuccessResponse("Deleted"));
    }
}
