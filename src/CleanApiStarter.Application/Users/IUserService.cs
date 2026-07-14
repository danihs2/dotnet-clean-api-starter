using CleanApiStarter.Application.Common;

namespace CleanApiStarter.Application.Users;

public interface IUserService
{
    Task<PagedResult<UserResponse>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<UserResponse> ChangeRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default);
}
