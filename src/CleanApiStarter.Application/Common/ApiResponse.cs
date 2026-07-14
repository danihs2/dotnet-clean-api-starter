namespace CleanApiStarter.Application.Common;

public sealed record ApiResponse<T>(
    bool Success,
    string Message,
    T? Data,
    IReadOnlyDictionary<string, string[]>? Errors,
    string TraceId)
{
    public static ApiResponse<T> Ok(T? data, string message, string traceId) =>
        new(true, message, data, null, traceId);

    public static ApiResponse<T> Fail(
        string message,
        string traceId,
        IReadOnlyDictionary<string, string[]>? errors = null) =>
        new(false, message, default, errors, traceId);
}
