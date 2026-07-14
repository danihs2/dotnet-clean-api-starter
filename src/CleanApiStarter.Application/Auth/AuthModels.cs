using System.ComponentModel.DataAnnotations;

namespace CleanApiStarter.Application.Auth;

public sealed record RegisterRequest(
    [Required, EmailAddress, MaxLength(320)] string Email,
    [Required, MinLength(8), MaxLength(128)] string Password);

public sealed record LoginRequest(
    [Required, EmailAddress, MaxLength(320)] string Email,
    [Required, MaxLength(128)] string Password);

public sealed record RefreshTokenRequest([Required, MaxLength(512)] string RefreshToken);

public sealed record RevokeRefreshTokenRequest([Required, MaxLength(512)] string RefreshToken);

public sealed record TokenResult(string AccessToken, DateTimeOffset ExpiresAtUtc);

public sealed record RefreshTokenResult(string Token, string TokenHash, DateTimeOffset ExpiresAtUtc);

public sealed record AuthResponse(
    Guid Id,
    string Email,
    string Role,
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc);

public sealed record CurrentUserResponse(Guid Id, string Email, string Role);
