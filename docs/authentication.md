# Authentication and authorization

## JWT configuration

Settings live under `Jwt` and may be overridden with environment variables:

```text
Jwt__Issuer
Jwt__Audience
Jwt__Secret
Jwt__ExpirationMinutes
Jwt__RefreshTokenExpirationDays
```

The secret must contain at least 32 characters. Configuration is validated at startup. Access tokens use HMAC-SHA256 and include subject, name identifier, email, role, and token ID claims. The default access-token lifetime is 60 minutes with a small validation clock skew. Refresh tokens expire after seven days by default.

## Password handling

Passwords are hashed through ASP.NET Core `PasswordHasher<User>`. Plaintext passwords are never persisted, placed in audit metadata, or logged. Registration requires at least eight characters with uppercase, lowercase, and numeric characters.

## Endpoints

`POST /api/auth/register` is public and always assigns `User`. Its request intentionally has no role property.

```json
{
  "email": "person@example.com",
  "password": "ValidPass123!"
}
```

`POST /api/auth/login` returns the user identity, role, access token, refresh token, and both UTC expirations. Invalid credentials return the same message whether the email is unknown, inactive, or the password is wrong.

`POST /api/auth/refresh` accepts a refresh token and returns a new access/refresh pair:

```json
{
  "refreshToken": "token-returned-by-login-or-registration"
}
```

Refresh tokens are single-use. A successful refresh revokes the submitted token and creates a replacement. Reusing a rotated token is treated as a possible credential theft event and revokes all remaining active refresh tokens for that user.

`POST /api/auth/revoke` accepts the same shape and revokes the token. It is idempotent and returns success for unknown or already revoked values so callers cannot probe token existence.

`GET /api/auth/me` requires a valid token and reloads the user from the database.

## Refresh-token storage

Refresh tokens contain 64 cryptographically random bytes and are returned only when issued. PostgreSQL stores their SHA-256 hashes, expiration, creation/revocation context, reason, and replacement identifier. Raw refresh tokens must be handled like credentials: never log them, persist them in audit metadata, or expose them in URLs.

Browser clients should prefer a secure, HttpOnly, SameSite cookie managed by their frontend/backend boundary. Non-browser clients need an operating-system credential store. This starter returns tokens in JSON so derived projects can choose the appropriate delivery mechanism.

## Roles and policies

- `User`: normal authenticated access.
- `SuperAdmin`: all protected starter endpoints, including user administration and audit reads.
- `AuthenticatedUser`: requires any valid authenticated identity.
- `SuperAdminOnly`: requires the `SuperAdmin` role claim.

Only SuperAdmin may call `PUT /api/users/{id}/role`. The service accepts only the two existing starter roles and audits accepted or rejected role changes.

## Development accounts

Development startup seeds `admin@example.com` and `user@example.com`, both with `ChangeMe123!`. Override them with `Seed__...` variables. Seed creation is idempotent and disabled outside Development.

## Production considerations

Use a secret manager, rotate signing keys, choose appropriate access/refresh lifetimes, add rate limiting and account lockout, periodically remove expired refresh-token records, and evaluate email verification and recovery flows based on project needs. Never deploy the development accounts.
