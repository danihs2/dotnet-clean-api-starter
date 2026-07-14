using System.ComponentModel.DataAnnotations;

namespace CleanApiStarter.Application.Users;

public sealed record UserResponse(
    Guid Id,
    string Email,
    string Role,
    bool IsActive,
    DateTimeOffset CreatedAtUtc);

public sealed record ChangeUserRoleRequest([Required, MaxLength(50)] string Role);
