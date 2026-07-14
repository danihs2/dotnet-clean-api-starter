using CleanApiStarter.Application.Common;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CleanApiStarter.Api.Extensions;

public static class HealthCheckResponse
{
    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        var data = new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration,
                description = entry.Value.Description
            })
        };
        return context.Response.WriteAsJsonAsync(ApiResponse<object>.Ok(data, "Health check completed.", context.TraceIdentifier));
    }
}
