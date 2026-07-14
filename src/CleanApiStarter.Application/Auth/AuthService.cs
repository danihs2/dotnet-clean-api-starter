using CleanApiStarter.Application.Abstractions;
using CleanApiStarter.Application.Auditing;
using CleanApiStarter.Application.Common;
using CleanApiStarter.Domain.Constants;
using CleanApiStarter.Domain.Entities;

namespace CleanApiStarter.Application.Auth;

public sealed class AuthService(
    IUserRepository users,
    IRoleRepository roles,
    IPasswordService passwords,
    IJwtTokenGenerator tokens,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenRepository refreshTokens,
    IAuditService audit,
    IRequestContext requestContext,
    IUnitOfWork unitOfWork) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ValidatePassword(request.Password);
        var normalizedEmail = NormalizeEmail(request.Email);

        if (await users.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken) is not null)
        {
            await audit.LogAsync(new AuditEvent(
                "UserRegistration", "User registration was rejected because the email already exists.", false,
                UserEmail: request.Email.Trim(), EntityName: nameof(User), ErrorCode: "email_already_exists"), cancellationToken);
            throw new ConflictException("A user with this email already exists.", "email_already_exists");
        }

        var userRole = await roles.GetByNameAsync(AppRoles.User, cancellationToken)
            ?? throw new InvalidOperationException("The default User role has not been initialized.");

        var user = new User
        {
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            PasswordHash = string.Empty,
            RoleId = userRole.Id,
            Role = userRole
        };
        user.PasswordHash = passwords.Hash(user, request.Password);

        await users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await audit.LogAsync(new AuditEvent(
            "UserCreated", "A user account was created through public registration.", true,
            user.Id, user.Email, AppRoles.User, nameof(User), user.Id.ToString()), cancellationToken);

        return await CreateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await users.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive || !passwords.Verify(user, user.PasswordHash, request.Password))
        {
            await audit.LogAsync(new AuditEvent(
                "LoginAttempt", "A login attempt failed.", false,
                user?.Id, request.Email.Trim(), user?.Role?.Name, nameof(User), user?.Id.ToString(),
                ErrorCode: "invalid_credentials"), cancellationToken);
            throw new UnauthorizedException("Invalid email or password.", "invalid_credentials");
        }

        await audit.LogAsync(new AuditEvent(
            "LoginAttempt", "A user logged in successfully.", true,
            user.Id, user.Email, user.Role.Name, nameof(User), user.Id.ToString()), cancellationToken);

        return await CreateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = refreshTokenGenerator.Hash(refreshToken);
        var storedToken = await refreshTokens.GetByHashAsync(tokenHash, cancellationToken);

        if (storedToken is null)
        {
            await audit.LogAsync(new AuditEvent(
                "RefreshTokenFailed", "A refresh attempt used an unknown token.", false,
                ErrorCode: "invalid_refresh_token"), cancellationToken);
            throw new UnauthorizedException("The refresh token is invalid.", "invalid_refresh_token");
        }

        if (storedToken.RevokedAtUtc is not null)
        {
            await RevokeAllActiveTokensAsync(storedToken.UserId, "Refresh token reuse detected.", cancellationToken);
            await audit.LogAsync(new AuditEvent(
                "RefreshTokenReuseDetected", "A revoked refresh token was reused; active tokens were revoked.", false,
                storedToken.UserId, storedToken.User.Email, storedToken.User.Role.Name,
                nameof(RefreshToken), storedToken.Id.ToString(), "refresh_token_reuse"), cancellationToken);
            throw new UnauthorizedException("The refresh token is no longer valid.", "refresh_token_reuse");
        }

        if (storedToken.ExpiresAtUtc <= DateTimeOffset.UtcNow || !storedToken.User.IsActive)
        {
            Revoke(storedToken, storedToken.User.IsActive ? "Refresh token expired." : "User is inactive.");
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await audit.LogAsync(new AuditEvent(
                "RefreshTokenFailed", "A refresh attempt used an expired or inactive token.", false,
                storedToken.UserId, storedToken.User.Email, storedToken.User.Role.Name,
                nameof(RefreshToken), storedToken.Id.ToString(), "invalid_refresh_token"), cancellationToken);
            throw new UnauthorizedException("The refresh token is no longer valid.", "invalid_refresh_token");
        }

        var replacement = refreshTokenGenerator.Generate();
        var replacementEntity = CreateRefreshToken(storedToken.User, replacement);
        Revoke(storedToken, "Replaced by token rotation.");
        storedToken.ReplacedByTokenId = replacementEntity.Id;
        await refreshTokens.AddAsync(replacementEntity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await audit.LogAsync(new AuditEvent(
            "RefreshTokenRotated", "A refresh token was rotated successfully.", true,
            storedToken.UserId, storedToken.User.Email, storedToken.User.Role.Name,
            nameof(RefreshToken), storedToken.Id.ToString(),
            Metadata: new { ReplacementTokenId = replacementEntity.Id }), cancellationToken);

        return CreateAuthResponse(storedToken.User, replacement);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = refreshTokenGenerator.Hash(refreshToken);
        var storedToken = await refreshTokens.GetByHashAsync(tokenHash, cancellationToken);

        if (storedToken is null)
        {
            return;
        }

        if (storedToken.RevokedAtUtc is null)
        {
            Revoke(storedToken, "Revoked by client request.");
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await audit.LogAsync(new AuditEvent(
                "RefreshTokenRevoked", "A refresh token was revoked by client request.", true,
                storedToken.UserId, storedToken.User.Email, storedToken.User.Role.Name,
                nameof(RefreshToken), storedToken.Id.ToString()), cancellationToken);
        }
    }

    public async Task<CurrentUserResponse> GetCurrentAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("The current user no longer exists.", "user_not_found");

        return new CurrentUserResponse(user.Id, user.Email, user.Role.Name);
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        var refreshToken = refreshTokenGenerator.Generate();
        await refreshTokens.AddAsync(CreateRefreshToken(user, refreshToken), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return CreateAuthResponse(user, refreshToken);
    }

    private AuthResponse CreateAuthResponse(User user, RefreshTokenResult refreshToken)
    {
        var accessToken = tokens.Generate(user);
        return new AuthResponse(
            user.Id, user.Email, user.Role.Name, accessToken.AccessToken, accessToken.ExpiresAtUtc,
            refreshToken.Token, refreshToken.ExpiresAtUtc);
    }

    private RefreshToken CreateRefreshToken(User user, RefreshTokenResult refreshToken) => new()
    {
        UserId = user.Id,
        User = user,
        TokenHash = refreshToken.TokenHash,
        ExpiresAtUtc = refreshToken.ExpiresAtUtc,
        CreatedByIp = requestContext.IpAddress
    };

    private async Task RevokeAllActiveTokensAsync(
        Guid userId,
        string reason,
        CancellationToken cancellationToken)
    {
        var activeTokens = await refreshTokens.GetActiveByUserIdAsync(userId, cancellationToken);
        foreach (var token in activeTokens)
        {
            Revoke(token, reason);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private void Revoke(RefreshToken token, string reason)
    {
        token.RevokedAtUtc = DateTimeOffset.UtcNow;
        token.RevokedByIp = requestContext.IpAddress;
        token.RevocationReason = reason;
        token.ConcurrencyStamp = Guid.NewGuid();
    }

    public static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    private static void ValidatePassword(string password)
    {
        var errors = new List<string>();
        if (!password.Any(char.IsUpper)) errors.Add("Password must contain an uppercase letter.");
        if (!password.Any(char.IsLower)) errors.Add("Password must contain a lowercase letter.");
        if (!password.Any(char.IsDigit)) errors.Add("Password must contain a number.");

        if (errors.Count > 0)
        {
            throw new ValidationException("Password validation failed.", new Dictionary<string, string[]>
            {
                ["password"] = errors.ToArray()
            });
        }
    }
}
