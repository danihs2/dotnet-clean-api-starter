using System.ComponentModel.DataAnnotations;

namespace CleanApiStarter.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required, MinLength(32)]
    public string Secret { get; init; } = string.Empty;

    [Range(1, 1440)]
    public int ExpirationMinutes { get; init; } = 60;

    [Range(1, 365)]
    public int RefreshTokenExpirationDays { get; init; } = 7;
}
