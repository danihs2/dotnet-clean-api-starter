using CleanApiStarter.Application.Abstractions;
using CleanApiStarter.Application.Auth;
using CleanApiStarter.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanApiStarter.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Tags("Auth")]
public sealed class AuthController(IAuthService authService, IRequestContext requestContext) : ControllerBase
{
    /// <summary>Registers a user with the User role.</summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<AuthResponse>.Ok(result, "User registered successfully.", HttpContext.TraceIdentifier));
    }

    /// <summary>Authenticates a user and returns a JWT access token.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful.", HttpContext.TraceIdentifier));
    }

    /// <summary>Rotates a valid refresh token and returns a new token pair.</summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.RefreshAsync(request.RefreshToken, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Token refreshed successfully.", HttpContext.TraceIdentifier));
    }

    /// <summary>Revokes a refresh token. The operation is idempotent.</summary>
    [AllowAnonymous]
    [HttpPost("revoke")]
    public async Task<ActionResult<ApiResponse<object>>> Revoke(
        RevokeRefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        await authService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Refresh token revoked.", HttpContext.TraceIdentifier));
    }

    /// <summary>Returns the authenticated user's profile.</summary>
    [Authorize(Policy = "AuthenticatedUser")]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<CurrentUserResponse>>> Me(CancellationToken cancellationToken)
    {
        var userId = requestContext.UserId
            ?? throw new Application.Common.UnauthorizedException("The access token does not contain a valid user identifier.");
        var result = await authService.GetCurrentAsync(userId, cancellationToken);
        return Ok(ApiResponse<CurrentUserResponse>.Ok(result, "Current user retrieved.", HttpContext.TraceIdentifier));
    }
}
