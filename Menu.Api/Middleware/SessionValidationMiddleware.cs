using System.Security.Claims;

using Menu.Infrastructure.Data;

namespace Menu.Api.Middleware;

public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;

    public SessionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var sid = context.User.FindFirstValue("sid");
        var jti = context.User.FindFirstValue("jti") ?? context.User.FindFirstValue(ClaimTypes.SerialNumber);

        if (!Guid.TryParse(sid, out var sessionId) || string.IsNullOrWhiteSpace(jti))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid session claims.");
            return;
        }

        var session = dbContext.UserSessions.FirstOrDefault(x => x.Id == sessionId);
        if (session is null || session.IsRevoked || session.RefreshTokenExpiresAt <= DateTime.UtcNow)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Session is invalid.");
            return;
        }

        if (!string.Equals(session.Jti, jti, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Session token mismatch.");
            return;
        }

        await _next(context);
    }
}
