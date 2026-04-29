using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Menu.Application.Common.Models;
using Menu.Application.DTOs.Restaurant;
using Menu.Application.Interfaces;
using Menu.Domain.Authorization;

namespace Menu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.SuperAdmin)]
public class RestaurantsController : ControllerBase
{
    private readonly IRestaurantService _service;

    public RestaurantsController(IRestaurantService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = PermissionNames.RestaurantsView)]
    public async Task<ActionResult<ApiResponse<PaginatedResult<RestaurantDto>>>> GetAll([FromQuery] QueryParameters query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return Ok(ApiResponse<PaginatedResult<RestaurantDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionNames.RestaurantsView)]
    public async Task<ActionResult<ApiResponse<RestaurantDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<RestaurantDto>.SuccessResponse(result));
    }

    [HttpPost]
    [Authorize(Policy = PermissionNames.RestaurantsCreate)]
    public async Task<ActionResult<ApiResponse<RestaurantDto>>> Create([FromBody] CreateRestaurantDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return Ok(ApiResponse<RestaurantDto>.SuccessResponse(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionNames.RestaurantsUpdate)]
    public async Task<ActionResult<ApiResponse<RestaurantDto>>> Update(Guid id, [FromBody] UpdateRestaurantDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return Ok(ApiResponse<RestaurantDto>.SuccessResponse(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionNames.RestaurantsDelete)]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Ok(ApiResponse<string>.SuccessResponse("Deleted"));
    }
}
