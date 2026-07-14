using System.Net;
using CleanApiStarter.Application.Auditing;
using CleanApiStarter.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CleanApiStarter.Tests.Integration;

public sealed class HealthAndAuditTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Theory]
    [InlineData("/health")]
    [InlineData("/api/health")]
    public async Task Health_endpoints_include_database_check(string path)
    {
        var response = await factory.CreateClient().GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("database", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Audit_service_persists_success_and_failure_events()
    {
        using var scope = factory.Services.CreateScope();
        var audit = scope.ServiceProvider.GetRequiredService<IAuditService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await audit.LogAsync(new AuditEvent("TestSuccess", "Success was recorded.", true));
        await Assert.ThrowsAsync<InvalidOperationException>(() => audit.ExecuteAsync<object>(
            new AuditEvent("TestFailure", "Failure was recorded.", false),
            _ => throw new InvalidOperationException("Expected test failure.")));

        Assert.Contains(context.AuditLogs, x => x.EventType == "TestSuccess" && x.Success);
        Assert.Contains(context.AuditLogs, x => x.EventType == "TestFailure" && !x.Success && x.ErrorMessage != null);
    }
}
