namespace CleanApiStarter.Application.Common;

public abstract class AppException(string message, string code, Exception? innerException = null) : Exception(message, innerException)
{
    public string Code { get; } = code;
}

public sealed class ConflictException(
    string message,
    string code = "conflict",
    Exception? innerException = null) : AppException(message, code, innerException);
public sealed class NotFoundException(string message, string code = "not_found") : AppException(message, code);
public sealed class UnauthorizedException(string message, string code = "unauthorized") : AppException(message, code);
public sealed class ForbiddenException(string message, string code = "forbidden") : AppException(message, code);
public sealed class ValidationException(
    string message,
    IReadOnlyDictionary<string, string[]> errors,
    string code = "validation_failed") : AppException(message, code)
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
