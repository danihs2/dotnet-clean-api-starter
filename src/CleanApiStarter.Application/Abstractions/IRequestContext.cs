namespace CleanApiStarter.Application.Abstractions;

public interface IRequestContext
{
    string TraceId { get; }
    Guid? UserId { get; }
    string? UserEmail { get; }
    string? Role { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}
