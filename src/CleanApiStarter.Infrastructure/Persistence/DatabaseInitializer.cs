using CleanApiStarter.Application.Abstractions;
using CleanApiStarter.Application.Auth;
using CleanApiStarter.Domain.Constants;
using CleanApiStarter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanApiStarter.Infrastructure.Persistence;

public sealed class DatabaseInitializer(
    AppDbContext context,
    IPasswordService passwordService,
    IOptions<SeedOptions> seedOptions,
    ILogger<DatabaseInitializer> logger)
{
    public async Task InitializeDevelopmentAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Applying database migrations for the Development environment");
        await context.Database.MigrateAsync(cancellationToken);

        var roles = new Dictionary<string, Role>(StringComparer.Ordinal);
        foreach (var roleName in AppRoles.All)
        {
            var role = await context.Roles.SingleOrDefaultAsync(x => x.Name == roleName, cancellationToken);
            if (role is null)
            {
                role = new Role { Name = roleName };
                context.Roles.Add(role);
            }
            roles[roleName] = role;
        }
        await context.SaveChangesAsync(cancellationToken);

        var options = seedOptions.Value;
        await EnsureUserAsync(options.SuperAdminEmail, options.SuperAdminPassword, roles[AppRoles.SuperAdmin], cancellationToken);
        await EnsureUserAsync(options.UserEmail, options.UserPassword, roles[AppRoles.User], cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureUserAsync(
        string email,
        string password,
        Role role,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = AuthService.NormalizeEmail(email);
        if (await context.Users.AnyAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken))
        {
            return;
        }

        var user = new User
        {
            Email = email.Trim(),
            NormalizedEmail = normalizedEmail,
            PasswordHash = string.Empty,
            Role = role,
            RoleId = role.Id
        };
        user.PasswordHash = passwordService.Hash(user, password);
        context.Users.Add(user);
        logger.LogInformation("Created Development seed user {Email} with role {Role}", user.Email, role.Name);
    }
}
