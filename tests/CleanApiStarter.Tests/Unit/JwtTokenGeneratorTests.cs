using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CleanApiStarter.Domain.Constants;
using CleanApiStarter.Domain.Entities;
using CleanApiStarter.Infrastructure.Authentication;
using Microsoft.Extensions.Options;

namespace CleanApiStarter.Tests.Unit;

public sealed class JwtTokenGeneratorTests
{
    [Fact]
    public void Generate_includes_identity_and_role_claims()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            Secret = "a-secure-test-secret-that-is-at-least-32-characters-long",
            ExpirationMinutes = 30
        });
        var role = new Role { Name = AppRoles.SuperAdmin };
        var user = new User
        {
            Email = "admin@example.com",
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            PasswordHash = "not-used",
            Role = role,
            RoleId = role.Id
        };

        var result = new JwtTokenGenerator(options).Generate(user);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);

        Assert.Equal("test-issuer", token.Issuer);
        Assert.Contains("test-audience", token.Audiences);
        Assert.Contains(token.Claims, claim => claim.Type == JwtRegisteredClaimNames.Sub && claim.Value == user.Id.ToString());
        Assert.Contains(token.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == AppRoles.SuperAdmin);
        Assert.True(result.ExpiresAtUtc > DateTimeOffset.UtcNow.AddMinutes(29));
    }

    [Fact]
    public void Default_roles_are_stable()
    {
        Assert.Equal(["SuperAdmin", "User"], AppRoles.All);
    }
}
