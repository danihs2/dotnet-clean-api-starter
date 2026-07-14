using System.Security.Claims;
using CleanApiStarter.Application.Abstractions;

namespace CleanApiStarter.Api.Services;

public sealed class HttpRequestContext(IHttpContextAccessor accessor) : IRequestContext
{
    private HttpContext? Context => accessor.HttpContext;

    public string TraceId => Context?.TraceIdentifier ?? string.Empty;
    public Guid? UserId => Guid.TryParse(Context?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    public string? UserEmail => Context?.User.FindFirstValue(ClaimTypes.Email);
    public string? Role => Context?.User.FindFirstValue(ClaimTypes.Role);
    public string? IpAddress => Context?.Connection.RemoteIpAddress?.ToString();
    public string? UserAgent => Context?.Request.Headers.UserAgent.ToString();
}
