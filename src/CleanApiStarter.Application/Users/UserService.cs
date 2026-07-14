using CleanApiStarter.Application.Abstractions;
using CleanApiStarter.Application.Auditing;
using CleanApiStarter.Application.Common;
using CleanApiStarter.Domain.Constants;
using CleanApiStarter.Domain.Entities;

namespace CleanApiStarter.Application.Users;

public sealed class UserService(
    IUserRepository users,
    IRoleRepository roles,
    IAuditService audit,
    IRequestContext requestContext,
    IUnitOfWork unitOfWork) : IUserService
{
    public async Task<PagedResult<UserResponse>> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await users.GetPageAsync(page, pageSize, cancellationToken);
        return new PagedResult<UserResponse>(result.Items.Select(Map).ToArray(), page, pageSize, result.TotalCount);
    }

    public async Task<UserResponse> ChangeRoleAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        var normalizedRole = AppRoles.All.FirstOrDefault(x => x.Equals(roleName.Trim(), StringComparison.OrdinalIgnoreCase));
        if (normalizedRole is null)
        {
            await audit.LogAsync(new AuditEvent(
                "UserRoleChanged", "A role change was rejected because the role is invalid.", false,
                requestContext.UserId, requestContext.UserEmail, requestContext.Role, nameof(User), userId.ToString(),
                ErrorCode: "invalid_role", Metadata: new { RequestedRole = roleName }), cancellationToken);
            throw new ValidationException("Role validation failed.", new Dictionary<string, string[]>
            {
                ["role"] = [$"Role must be one of: {string.Join(", ", AppRoles.All)}."]
            });
        }

        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User was not found.", "user_not_found");
        var role = await roles.GetByNameAsync(normalizedRole, cancellationToken)
            ?? throw new InvalidOperationException($"The {normalizedRole} role has not been initialized.");

        var previousRole = user.Role.Name;
        user.Role = role;
        user.RoleId = role.Id;
        user.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await audit.LogAsync(new AuditEvent(
            "UserRoleChanged", "A user's role was changed.", true,
            requestContext.UserId, requestContext.UserEmail, requestContext.Role, nameof(User), user.Id.ToString(),
            Metadata: new { PreviousRole = previousRole, NewRole = role.Name }), cancellationToken);

        return Map(user);
    }

    private static UserResponse Map(User user) =>
        new(user.Id, user.Email, user.Role.Name, user.IsActive, user.CreatedAtUtc);
}
