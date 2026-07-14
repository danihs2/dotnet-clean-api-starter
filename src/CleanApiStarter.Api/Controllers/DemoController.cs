using CleanApiStarter.Application.Auditing;
using CleanApiStarter.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanApiStarter.Api.Controllers;

[ApiController]
[Route("api/demo")]
[Tags("Demo")]
public sealed class DemoController(IAuditService auditService) : ControllerBase
{
    /// <summary>A public endpoint that does not require a token.</summary>
    [AllowAnonymous]
    [HttpGet("public")]
    public ActionResult<ApiResponse<object>> Public() =>
        Ok(ApiResponse<object>.Ok(new { access = "public" }, "Public endpoint reached.", HttpContext.TraceIdentifier));

    /// <summary>An endpoint for any authenticated user.</summary>
    [Authorize(Policy = "AuthenticatedUser")]
    [HttpGet("authenticated")]
    public async Task<ActionResult<ApiResponse<object>>> Authenticated(CancellationToken cancellationToken)
    {
        await auditService.LogAsync(new AuditEvent(
            "ProtectedDemoAction", "The authenticated demo endpoint was accessed.", true,
            EntityName: "Demo", EntityId: "authenticated"), cancellationToken);
        return Ok(ApiResponse<object>.Ok(
            new { access = "authenticated" }, "Authenticated endpoint reached.", HttpContext.TraceIdentifier));
    }

    /// <summary>An endpoint restricted to the SuperAdmin role.</summary>
    [Authorize(Policy = "SuperAdminOnly")]
    [HttpGet("super-admin")]
    public async Task<ActionResult<ApiResponse<object>>> SuperAdmin(CancellationToken cancellationToken)
    {
        await auditService.LogAsync(new AuditEvent(
            "ProtectedDemoAction", "The SuperAdmin demo endpoint was accessed.", true,
            EntityName: "Demo", EntityId: "super-admin"), cancellationToken);
        return Ok(ApiResponse<object>.Ok(
            new { access = "super-admin" }, "SuperAdmin endpoint reached.", HttpContext.TraceIdentifier));
    }
}
