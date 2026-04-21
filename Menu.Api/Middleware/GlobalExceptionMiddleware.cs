using System.Net;
using System.Text.Json;

using FluentValidation;

using Menu.Application.Common.Exceptions;
using Menu.Application.Common.Models;

namespace Menu.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = exception switch
        {
            NotFoundException => ApiResponse<object>.FailResponse(exception.Message, (int)HttpStatusCode.NotFound),
            BadRequestException => ApiResponse<object>.FailResponse(exception.Message, (int)HttpStatusCode.BadRequest),
            UnauthorizedException => ApiResponse<object>.FailResponse(exception.Message, (int)HttpStatusCode.Unauthorized),
            ForbiddenException => ApiResponse<object>.FailResponse(exception.Message, (int)HttpStatusCode.Forbidden),
            ValidationException validationException => new ApiResponse<object>
            {
                Success = false,
                Message = "Validation failed.",
                StatusCode = (int)HttpStatusCode.BadRequest,
                Errors = validationException.Errors.Select(x => x.ErrorMessage).ToList()
            },
            _ => ApiResponse<object>.FailResponse("Internal Server Error", (int)HttpStatusCode.InternalServerError)
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.StatusCode;
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
