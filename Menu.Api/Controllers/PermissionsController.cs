using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Menu.Application.Common.Models;
using Menu.Application.DTOs.Permission;
using Menu.Application.Interfaces;
using Menu.Domain.Authorization;

namespace Menu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.SuperAdmin)]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _service;

    public PermissionsController(IPermissionService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = PermissionNames.PermissionsView)]
    public async Task<ActionResult<ApiResponse<PaginatedResult<PermissionDto>>>> GetAll([FromQuery] QueryParameters query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return Ok(ApiResponse<PaginatedResult<PermissionDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionNames.PermissionsView)]
    public async Task<ActionResult<ApiResponse<PermissionDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<PermissionDto>.SuccessResponse(result));
    }

    [HttpPost]
    [Authorize(Policy = PermissionNames.PermissionsCreate)]
    public async Task<ActionResult<ApiResponse<PermissionDto>>> Create([FromBody] CreatePermissionDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return Ok(ApiResponse<PermissionDto>.SuccessResponse(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionNames.PermissionsUpdate)]
    public async Task<ActionResult<ApiResponse<PermissionDto>>> Update(Guid id, [FromBody] UpdatePermissionDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return Ok(ApiResponse<PermissionDto>.SuccessResponse(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionNames.PermissionsDelete)]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Ok(ApiResponse<string>.SuccessResponse("Deleted"));
    }
}
