using CleanApiStarter.Application.Auditing;
using CleanApiStarter.Application.Common;
using CleanApiStarter.Domain.Entities;

namespace CleanApiStarter.Application.Abstractions;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    Task<PagedResult<AuditLog>> GetPageAsync(AuditQuery query, CancellationToken cancellationToken = default);
}
