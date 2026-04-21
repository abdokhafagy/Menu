using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Menu.Api.Filters;
using Menu.Application.Common.Models;
using Menu.Application.DTOs.Permission;
using Menu.Application.Interfaces;

namespace Menu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _service;

    public PermissionsController(IPermissionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<PermissionDto>>>> GetAll([FromQuery] QueryParameters query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return Ok(ApiResponse<PaginatedResult<PermissionDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PermissionDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<PermissionDto>.SuccessResponse(result));
    }

    [HttpPost]
    [RequirePermission("permissions.create")]
    public async Task<ActionResult<ApiResponse<PermissionDto>>> Create([FromBody] CreatePermissionDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return Ok(ApiResponse<PermissionDto>.SuccessResponse(result));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("permissions.update")]
    public async Task<ActionResult<ApiResponse<PermissionDto>>> Update(Guid id, [FromBody] UpdatePermissionDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return Ok(ApiResponse<PermissionDto>.SuccessResponse(result));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("permissions.delete")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Ok(ApiResponse<string>.SuccessResponse("Deleted"));
    }
}
