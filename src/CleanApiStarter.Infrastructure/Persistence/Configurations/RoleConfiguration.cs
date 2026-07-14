using CleanApiStarter.Domain.Constants;
using CleanApiStarter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanApiStarter.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasData(
            new Role
            {
                Id = Guid.Parse("4c31d31d-ccb2-4f76-96a7-0ab4a2689110"),
                Name = AppRoles.SuperAdmin,
                CreatedAtUtc = DateTimeOffset.UnixEpoch
            },
            new Role
            {
                Id = Guid.Parse("cb29fabd-4d84-461d-b4ba-4666ad60187f"),
                Name = AppRoles.User,
                CreatedAtUtc = DateTimeOffset.UnixEpoch
            });
    }
}
