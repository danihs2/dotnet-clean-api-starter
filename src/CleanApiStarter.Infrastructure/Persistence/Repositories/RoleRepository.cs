using CleanApiStarter.Application.Abstractions;
using CleanApiStarter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanApiStarter.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository(AppDbContext context) : IRoleRepository
{
    public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        context.Roles.SingleOrDefaultAsync(x => x.Name == name, cancellationToken);
}
