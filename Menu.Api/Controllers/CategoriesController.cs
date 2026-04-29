using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Menu.Application.Common.Models;
using Menu.Application.DTOs.Category;
using Menu.Application.Interfaces;
using Menu.Domain.Authorization;

namespace Menu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.Admin},{RoleNames.Manager}")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = PermissionNames.CategoriesView)]
    public async Task<ActionResult<ApiResponse<PaginatedResult<CategoryDto>>>> GetAll([FromQuery] QueryParameters query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return Ok(ApiResponse<PaginatedResult<CategoryDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionNames.CategoriesView)]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<CategoryDto>.SuccessResponse(result));
    }

    [HttpGet("/api/menus/{menuId:guid}/categories")]
    [Authorize(Policy = PermissionNames.CategoriesView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CategoryDto>>>> GetByMenu(Guid menuId, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(new QueryParameters { PageNumber = 1, PageSize = 100 }, ct);
        var filtered = result.Data.Where(x => x.MenuId == menuId).OrderBy(x => x.DisplayOrder).ToList();
        return Ok(ApiResponse<IReadOnlyList<CategoryDto>>.SuccessResponse(filtered));
    }

    [HttpPost]
    [Authorize(Policy = PermissionNames.CategoriesCreate)]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create([FromBody] CreateCategoryDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return Ok(ApiResponse<CategoryDto>.SuccessResponse(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionNames.CategoriesUpdate)]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(Guid id, [FromBody] UpdateCategoryDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return Ok(ApiResponse<CategoryDto>.SuccessResponse(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionNames.CategoriesDelete)]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Ok(ApiResponse<string>.SuccessResponse("Deleted"));
    }
}
