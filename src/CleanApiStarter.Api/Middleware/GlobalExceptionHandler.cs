using CleanApiStarter.Application.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace CleanApiStarter.Api.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, message, errors, code) = exception switch
        {
            ValidationException validation => (
                StatusCodes.Status400BadRequest,
                validation.Message,
                validation.Errors,
                validation.Code),
            UnauthorizedException unauthorized => (
                StatusCodes.Status401Unauthorized,
                unauthorized.Message,
                null,
                unauthorized.Code),
            ForbiddenException forbidden => (
                StatusCodes.Status403Forbidden,
                forbidden.Message,
                null,
                forbidden.Code),
            NotFoundException notFound => (
                StatusCodes.Status404NotFound,
                notFound.Message,
                null,
                notFound.Code),
            ConflictException conflict => (
                StatusCodes.Status409Conflict,
                conflict.Message,
                null,
                conflict.Code),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                null,
                "unexpected_error")
        };

        logger.LogError(exception,
            "Request {TraceId} failed with status {StatusCode} and error code {ErrorCode}",
            httpContext.TraceIdentifier, statusCode, code);

        var responseErrors = errors ?? new Dictionary<string, string[]> { ["code"] = [code] };
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(
            ApiResponse<object>.Fail(message, httpContext.TraceIdentifier, responseErrors),
            cancellationToken);
        return true;
    }
}
