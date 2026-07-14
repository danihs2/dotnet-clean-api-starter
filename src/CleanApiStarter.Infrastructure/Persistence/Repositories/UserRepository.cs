using CleanApiStarter.Application.Abstractions;
using CleanApiStarter.Application.Common;
using CleanApiStarter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanApiStarter.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext context) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Users.Include(x => x.Role).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        context.Users.Include(x => x.Role)
            .SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);

    public async Task<PagedResult<User>> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = context.Users.AsNoTracking().Include(x => x.Role).OrderBy(x => x.Email);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(cancellationToken);
        return new PagedResult<User>(items, page, pageSize, total);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        context.Users.AddAsync(user, cancellationToken).AsTask();
}
