using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CleanApiStarter.Application.Auth;
using CleanApiStarter.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CleanApiStarter.Tests.Integration;

public sealed class AuthAndAuthorizationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Public_endpoint_allows_anonymous_requests()
    {
        var response = await _client.GetAsync("/api/demo/public");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
    }

    [Fact]
    public async Task Protected_endpoint_rejects_anonymous_requests()
    {
        var response = await _client.GetAsync("/api/demo/authenticated");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(json.GetProperty("success").GetBoolean());
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task User_can_access_authenticated_but_not_super_admin_endpoint()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await Login("user@example.com"));

        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api/demo/authenticated")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await _client.GetAsync("/api/demo/super-admin")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await _client.GetAsync("/api/users")).StatusCode);
    }

    [Fact]
    public async Task Super_admin_can_access_admin_endpoints()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await Login("admin@example.com"));

        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api/demo/super-admin")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api/users")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api/audit")).StatusCode);
    }

    [Fact]
    public async Task Registration_always_creates_a_user_role_and_me_returns_it()
    {
        var email = $"new-{Guid.NewGuid():N}@example.com";
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "ValidPass123!"));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var data = json.GetProperty("data");
        Assert.Equal("User", data.GetProperty("role").GetString());

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", data.GetProperty("accessToken").GetString());
        var me = await _client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        var meJson = await me.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("User", meJson.GetProperty("data").GetProperty("role").GetString());
    }

    [Fact]
    public async Task Invalid_credentials_return_safe_unauthorized_response()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("user@example.com", "wrong"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("stack", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PasswordHash", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Super_admin_can_change_a_registered_users_role()
    {
        var email = $"role-{Guid.NewGuid():N}@example.com";
        using var anonymousClient = factory.CreateClient();
        var registration = await anonymousClient.PostAsJsonAsync(
            "/api/auth/register", new RegisterRequest(email, "ValidPass123!"));
        registration.EnsureSuccessStatusCode();
        var registrationJson = await registration.Content.ReadFromJsonAsync<JsonElement>();
        var userId = registrationJson.GetProperty("data").GetProperty("id").GetGuid();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await Login("admin@example.com"));
        var response = await _client.PutAsJsonAsync($"/api/users/{userId}/role", new { role = "SuperAdmin" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("SuperAdmin", json.GetProperty("data").GetProperty("role").GetString());
    }

    [Fact]
    public async Task Duplicate_registration_returns_conflict()
    {
        var email = $"duplicate-{Guid.NewGuid():N}@example.com";
        var request = new RegisterRequest(email, "ValidPass123!");

        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/auth/register", request)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _client.PostAsJsonAsync("/api/auth/register", request)).StatusCode);
    }

    [Fact]
    public async Task Refresh_rotates_tokens_and_reuse_revokes_the_replacement()
    {
        using var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest("user@example.com", "ChangeMe123!"));
        login.EnsureSuccessStatusCode();
        var loginJson = await login.Content.ReadFromJsonAsync<JsonElement>();
        var originalRefreshToken = loginJson.GetProperty("data").GetProperty("refreshToken").GetString()!;

        var refresh = await client.PostAsJsonAsync(
            "/api/auth/refresh", new RefreshTokenRequest(originalRefreshToken));
        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
        var refreshJson = await refresh.Content.ReadFromJsonAsync<JsonElement>();
        var replacementRefreshToken = refreshJson.GetProperty("data").GetProperty("refreshToken").GetString()!;
        var replacementAccessToken = refreshJson.GetProperty("data").GetProperty("accessToken").GetString()!;
        Assert.NotEqual(originalRefreshToken, replacementRefreshToken);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", replacementAccessToken);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/auth/me")).StatusCode);
        client.DefaultRequestHeaders.Authorization = null;

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync(
            "/api/auth/refresh", new RefreshTokenRequest(originalRefreshToken))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync(
            "/api/auth/refresh", new RefreshTokenRequest(replacementRefreshToken))).StatusCode);
    }

    [Fact]
    public async Task Revoke_is_idempotent_and_prevents_refresh()
    {
        using var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest("admin@example.com", "ChangeMe123!"));
        login.EnsureSuccessStatusCode();
        var loginJson = await login.Content.ReadFromJsonAsync<JsonElement>();
        var refreshToken = loginJson.GetProperty("data").GetProperty("refreshToken").GetString()!;
        var request = new RevokeRefreshTokenRequest(refreshToken);

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/auth/revoke", request)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/auth/revoke", request)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync(
            "/api/auth/refresh", new RefreshTokenRequest(refreshToken))).StatusCode);
    }

    [Fact]
    public async Task Refresh_tokens_are_stored_only_as_hashes()
    {
        using var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest("user@example.com", "ChangeMe123!"));
        login.EnsureSuccessStatusCode();
        var loginJson = await login.Content.ReadFromJsonAsync<JsonElement>();
        var refreshToken = loginJson.GetProperty("data").GetProperty("refreshToken").GetString()!;

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.DoesNotContain(context.RefreshTokens, token => token.TokenHash == refreshToken);
        Assert.All(context.RefreshTokens, token => Assert.Equal(64, token.TokenHash.Length));
    }

    private async Task<string> Login(string email)
    {
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "ChangeMe123!"));
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("data").GetProperty("accessToken").GetString()!;
    }
}
