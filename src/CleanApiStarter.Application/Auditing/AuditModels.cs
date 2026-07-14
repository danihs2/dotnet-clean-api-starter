namespace CleanApiStarter.Application.Auditing;

public sealed record AuditEvent(
    string EventType,
    string Message,
    bool Success,
    Guid? UserId = null,
    string? UserEmail = null,
    string? Role = null,
    string? EntityName = null,
    string? EntityId = null,
    string? ErrorCode = null,
    string? ErrorMessage = null,
    object? Metadata = null);

public sealed record AuditQuery(int Page = 1, int PageSize = 20, string? EventType = null, bool? Success = null);

public sealed record AuditLogResponse(
    Guid Id,
    string EventType,
    string? EntityName,
    string? EntityId,
    Guid? UserId,
    string? UserEmail,
    string? Role,
    string? IpAddress,
    string? UserAgent,
    string Message,
    string? MetadataJson,
    DateTimeOffset CreatedAtUtc,
    bool Success,
    string? ErrorCode,
    string? ErrorMessage);
