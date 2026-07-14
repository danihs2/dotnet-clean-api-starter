using CleanApiStarter.Application.Auditing;
using CleanApiStarter.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanApiStarter.Api.Controllers;

[ApiController]
[Route("api/audit")]
[Tags("Audit")]
[Authorize(Policy = "SuperAdminOnly")]
public sealed class AuditController(IAuditService auditService) : ControllerBase
{
    /// <summary>Returns recent audit events with optional filters.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AuditLogResponse>>>> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? eventType = null,
        [FromQuery] bool? success = null,
        CancellationToken cancellationToken = default)
    {
        var result = await auditService.GetPageAsync(
            new AuditQuery(page, pageSize, eventType, success), cancellationToken);
        return Ok(ApiResponse<PagedResult<AuditLogResponse>>.Ok(
            result, "Audit logs retrieved.", HttpContext.TraceIdentifier));
    }
}
