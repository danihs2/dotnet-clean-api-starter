using CleanApiStarter.Application.Common;
using CleanApiStarter.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanApiStarter.Api.Controllers;

[ApiController]
[Route("api/users")]
[Tags("Users")]
[Authorize(Policy = "SuperAdminOnly")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    /// <summary>Lists users. Requires the SuperAdmin role.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UserResponse>>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await userService.GetPageAsync(page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<UserResponse>>.Ok(result, "Users retrieved.", HttpContext.TraceIdentifier));
    }

    /// <summary>Changes a user's role. Requires the SuperAdmin role.</summary>
    [HttpPut("{id:guid}/role")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> ChangeRole(
        Guid id,
        ChangeUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await userService.ChangeRoleAsync(id, request.Role, cancellationToken);
        return Ok(ApiResponse<UserResponse>.Ok(result, "User role changed.", HttpContext.TraceIdentifier));
    }
}
