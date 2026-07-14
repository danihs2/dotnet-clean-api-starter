using CleanApiStarter.Application.Abstractions;
using CleanApiStarter.Application.Auditing;
using CleanApiStarter.Application.Common;
using CleanApiStarter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanApiStarter.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository(AppDbContext context) : IAuditLogRepository
{
    public Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default) =>
        context.AuditLogs.AddAsync(auditLog, cancellationToken).AsTask();

    public async Task<PagedResult<AuditLog>> GetPageAsync(
        AuditQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var source = context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.EventType))
        {
            source = source.Where(x => x.EventType == query.EventType);
        }

        if (query.Success.HasValue)
        {
            source = source.Where(x => x.Success == query.Success.Value);
        }

        var total = await source.CountAsync(cancellationToken);
        var items = await source.OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<AuditLog>(items, page, pageSize, total);
    }
}
