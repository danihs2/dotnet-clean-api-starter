using CleanApiStarter.Domain.Common;

namespace CleanApiStarter.Domain.Entities;

public sealed class Role : AuditableEntity
{
    public required string Name { get; set; }
    public ICollection<User> Users { get; set; } = [];
}
