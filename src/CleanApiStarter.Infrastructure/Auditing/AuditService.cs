using System.Text.Json;
using CleanApiStarter.Application.Abstractions;
using CleanApiStarter.Application.Auditing;
using CleanApiStarter.Application.Common;
using CleanApiStarter.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CleanApiStarter.Infrastructure.Auditing;

public sealed class AuditService(
    IAuditLogRepository repository,
    IUnitOfWork unitOfWork,
    IRequestContext requestContext,
    ILogger<AuditService> logger) : IAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task LogAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = new AuditLog
            {
                EventType = auditEvent.EventType,
                EntityName = auditEvent.EntityName,
                EntityId = auditEvent.EntityId,
                UserId = auditEvent.UserId ?? requestContext.UserId,
                UserEmail = auditEvent.UserEmail ?? requestContext.UserEmail,
                Role = auditEvent.Role ?? requestContext.Role,
                IpAddress = requestContext.IpAddress,
                UserAgent = requestContext.UserAgent,
                Message = auditEvent.Message,
                MetadataJson = auditEvent.Metadata is null ? null : JsonSerializer.Serialize(auditEvent.Metadata, JsonOptions),
                Success = auditEvent.Success,
                ErrorCode = auditEvent.ErrorCode,
                ErrorMessage = auditEvent.ErrorMessage
            };

            await repository.AddAsync(auditLog, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation(
                "Audit event {EventType} recorded with success {Success} for entity {EntityName} {EntityId}",
                auditLog.EventType, auditLog.Success, auditLog.EntityName, auditLog.EntityId);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to persist audit event {EventType}", auditEvent.EventType);
        }
    }

    public async Task<T> ExecuteAsync<T>(
        AuditEvent auditEvent,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await operation(cancellationToken);
            await LogAsync(auditEvent with { Success = true }, cancellationToken);
            return result;
        }
        catch (Exception exception)
        {
            await LogAsync(auditEvent with
            {
                Success = false,
                ErrorCode = exception is AppException appException ? appException.Code : "unhandled_error",
                ErrorMessage = exception.Message
            }, cancellationToken);
            throw;
        }
    }

    public async Task<PagedResult<AuditLogResponse>> GetPageAsync(
        AuditQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await repository.GetPageAsync(query, cancellationToken);
        return new PagedResult<AuditLogResponse>(result.Items.Select(Map).ToArray(), result.Page, result.PageSize, result.TotalCount);
    }

    private static AuditLogResponse Map(AuditLog log) => new(
        log.Id, log.EventType, log.EntityName, log.EntityId, log.UserId, log.UserEmail, log.Role,
        log.IpAddress, log.UserAgent, log.Message, log.MetadataJson, log.CreatedAtUtc,
        log.Success, log.ErrorCode, log.ErrorMessage);
}
