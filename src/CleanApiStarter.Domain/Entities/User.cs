using CleanApiStarter.Domain.Common;

namespace CleanApiStarter.Domain.Entities;

public sealed class User : AuditableEntity
{
    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }
    public required string PasswordHash { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
