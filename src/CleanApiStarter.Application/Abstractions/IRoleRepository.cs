using CleanApiStarter.Domain.Entities;

namespace CleanApiStarter.Application.Abstractions;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
