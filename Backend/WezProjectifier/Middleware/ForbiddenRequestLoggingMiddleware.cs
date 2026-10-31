using System.Security.Claims;

namespace WezProjectifier.Middleware;

public sealed class ForbiddenRequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ForbiddenRequestLoggingMiddleware> _logger;

    public ForbiddenRequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<ForbiddenRequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.Response.StatusCode != StatusCodes.Status403Forbidden)
        {
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        _logger.LogWarning(
            "Forbidden request. UserId: {UserId}, ClientIp: {ClientIp}, Method: {Method}, Path: {Path}",
            userId,
            clientIp,
            context.Request.Method,
            context.Request.Path);
    }
}
