using System.Security.Cryptography;
using System.Text;
using CleanApiStarter.Application.Abstractions;
using CleanApiStarter.Application.Auth;
using Microsoft.Extensions.Options;

namespace CleanApiStarter.Infrastructure.Authentication;

public sealed class RefreshTokenGenerator(IOptions<JwtOptions> options) : IRefreshTokenGenerator
{
    private readonly JwtOptions _options = options.Value;

    public RefreshTokenResult Generate()
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        return new RefreshTokenResult(
            token,
            Hash(token),
            DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenExpirationDays));
    }

    public string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
