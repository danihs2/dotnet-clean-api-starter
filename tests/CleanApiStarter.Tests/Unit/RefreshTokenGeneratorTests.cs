using CleanApiStarter.Infrastructure.Authentication;
using Microsoft.Extensions.Options;

namespace CleanApiStarter.Tests.Unit;

public sealed class RefreshTokenGeneratorTests
{
    [Fact]
    public void Generate_creates_unique_tokens_and_sha256_hashes()
    {
        var generator = new RefreshTokenGenerator(Options.Create(new JwtOptions
        {
            Issuer = "test",
            Audience = "test",
            Secret = "a-secure-test-secret-that-is-at-least-32-characters-long",
            RefreshTokenExpirationDays = 14
        }));

        var first = generator.Generate();
        var second = generator.Generate();

        Assert.NotEqual(first.Token, second.Token);
        Assert.Equal(64, first.TokenHash.Length);
        Assert.Equal(first.TokenHash, generator.Hash(first.Token));
        Assert.True(first.ExpiresAtUtc > DateTimeOffset.UtcNow.AddDays(13));
    }
}
