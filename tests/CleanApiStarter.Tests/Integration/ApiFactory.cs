using CleanApiStarter.Application.Abstractions;
using CleanApiStarter.Application.Auth;
using CleanApiStarter.Domain.Constants;
using CleanApiStarter.Domain.Entities;
using CleanApiStarter.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace CleanApiStarter.Tests.Integration;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"clean-api-starter-tests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
                ["Jwt:Issuer"] = "CleanApiStarter.Tests",
                ["Jwt:Audience"] = "CleanApiStarter.Tests.Client",
                ["Jwt:Secret"] = "a-secure-test-secret-that-is-at-least-32-characters-long",
                ["Jwt:ExpirationMinutes"] = "60",
                ["Jwt:RefreshTokenExpirationDays"] = "7",
                ["Logging:LogLevel:Microsoft.AspNetCore.Authentication"] = "Debug"
            });
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
        Seed(context, scope.ServiceProvider.GetRequiredService<IPasswordService>());
        return host;
    }

    private static void Seed(AppDbContext context, IPasswordService passwords)
    {
        if (context.Users.Any()) return;

        var adminRole = context.Roles.SingleOrDefault(x => x.Name == AppRoles.SuperAdmin)
            ?? new Role { Name = AppRoles.SuperAdmin };
        var userRole = context.Roles.SingleOrDefault(x => x.Name == AppRoles.User)
            ?? new Role { Name = AppRoles.User };
        if (context.Entry(adminRole).State == EntityState.Detached) context.Roles.Add(adminRole);
        if (context.Entry(userRole).State == EntityState.Detached) context.Roles.Add(userRole);

        context.Users.Add(CreateUser("admin@example.com", "ChangeMe123!", adminRole, passwords));
        context.Users.Add(CreateUser("user@example.com", "ChangeMe123!", userRole, passwords));
        context.SaveChanges();
    }

    private static User CreateUser(string email, string password, Role role, IPasswordService passwords)
    {
        var user = new User
        {
            Email = email,
            NormalizedEmail = AuthService.NormalizeEmail(email),
            PasswordHash = string.Empty,
            Role = role,
            RoleId = role.Id
        };
        user.PasswordHash = passwords.Hash(user, password);
        return user;
    }
}
