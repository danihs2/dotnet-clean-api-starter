using CleanApiStarter.Application.Common;

namespace CleanApiStarter.Application.Auditing;

public interface IAuditService
{
    Task LogAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
    Task<T> ExecuteAsync<T>(
        AuditEvent auditEvent,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);
    Task<PagedResult<AuditLogResponse>> GetPageAsync(AuditQuery query, CancellationToken cancellationToken = default);
}
