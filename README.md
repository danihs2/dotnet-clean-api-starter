# CleanApiStarter

A reusable, production-minded ASP.NET Core Web API starter built on .NET 10 and a lightweight Clean Architecture. It provides authentication, role-based authorization, PostgreSQL persistence, auditing, health checks, structured errors, Swagger, Docker-based local development, and tests without imposing a business domain.

## Included

- ASP.NET Core 10 controller-based Web API.
- Domain, Application, Infrastructure, and API layers.
- Entity Framework Core with PostgreSQL and an initial migration.
- JWT bearer authentication and secure password hashing.
- Rotating, revocable refresh tokens stored only as SHA-256 hashes.
- `SuperAdmin` and `User` roles.
- Public user registration and protected user administration.
- Persistent audit events for important success and failure paths.
- Consistent API responses, correlation IDs, and centralized exception handling.
- JSON console logging with structured fields.
- Database-aware health checks at `/health` and `/api/health`.
- Swagger/OpenAPI with JWT support, tags, and collapsed sections.
- Development-only Docker Compose for PostgreSQL and the API.
- xUnit unit and integration tests.
- MIT license and public-repository documentation.

## Requirements

- .NET SDK 10.0.109 or another compatible .NET 10 patch SDK.
- PostgreSQL 18 or a compatible supported version.
- Docker and Docker Compose for the container workflow.

## Project structure

```text
src/
  CleanApiStarter.Api
  CleanApiStarter.Application
  CleanApiStarter.Domain
  CleanApiStarter.Infrastructure
tests/
  CleanApiStarter.Tests
docs/
  docker/
```

See [Architecture](docs/architecture.md) for dependency and responsibility details.

## Quick start with Docker

This Compose file is for local development only.

```bash
cp .env.example .env
docker compose -f docker-compose.dev.yml up --build -d
docker compose -f docker-compose.dev.yml logs -f api
```

Development migrations and seed data are applied automatically when the API starts. Open:

- Swagger: <http://localhost:8080/swagger>
- Health: <http://localhost:8080/health>
- API health: <http://localhost:8080/api/health>

If `API_HTTP_PORT` is changed in `.env`, use that port instead. See [Docker development](docs/docker/development.md), [commands](docs/docker/commands.md), and [troubleshooting](docs/docker/troubleshooting.md).

## Run locally without the API container

Start PostgreSQL, either from an existing installation or through Compose:

```bash
cp .env.example .env
docker compose -f docker-compose.dev.yml up -d postgres
dotnet tool restore
dotnet restore
dotnet run --project src/CleanApiStarter.Api --launch-profile http
```

The default local URL is <http://localhost:8080>. In `Development`, the API loads the root `.env` file without overwriting variables already provided by the operating system. Secrets and the connection string are intentionally absent from committed appsettings, so `.env` is the single local file to configure.

```bash
export Jwt__Secret='replace-with-a-local-secret-at-least-32-characters-long'
dotnet run --project src/CleanApiStarter.Api --launch-profile http
```

More detail is available in [Local development](docs/local-development.md).

## Database migrations

Development startup applies pending migrations and idempotent seed data automatically. Create and apply migrations manually with:

```bash
dotnet tool restore
dotnet ef migrations add MigrationName \
  --project src/CleanApiStarter.Infrastructure \
  --startup-project src/CleanApiStarter.Api \
  --output-dir Persistence/Migrations

dotnet ef database update \
  --project src/CleanApiStarter.Infrastructure \
  --startup-project src/CleanApiStarter.Api
```

Automatic migration is deliberately limited to `Development`. Use an explicit reviewed migration step in every other environment.

## Authentication and default users

Development startup creates these idempotent test accounts:

| Role | Email | Password |
| --- | --- | --- |
| `SuperAdmin` | `admin@example.com` | `ChangeMe123!` |
| `User` | `user@example.com` | `ChangeMe123!` |

These credentials are public, local-only values. They are never seeded outside `Development` and must not be reused in a deployed environment.

Core endpoints:

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/revoke
GET  /api/auth/me
GET  /api/users
PUT  /api/users/{id}/role
GET  /api/audit
```

Login and registration return an access token plus a seven-day refresh token. Refreshing rotates the token; reusing a rotated token revokes the user's remaining active refresh tokens. Public registration always assigns the `User` role. Only `SuperAdmin` can list users, change roles, view audits, or call the SuperAdmin demo. See [Authentication](docs/authentication.md).

## Authorization demo

```text
GET /api/demo/public       anonymous
GET /api/demo/authenticated any valid JWT
GET /api/demo/super-admin  SuperAdmin only
```

Use the `Authorize` button in Swagger and paste the access token returned by login.

## API response format

Successful and failed application responses use a common shape:

```json
{
  "success": true,
  "message": "Operation completed.",
  "data": {},
  "errors": null,
  "traceId": "request-correlation-id"
}
```

Clients may send `X-Correlation-ID`; otherwise the API creates one and returns it in the header and body. Internal exceptions are logged but stack traces are not returned.

## Tests

```bash
dotnet test CleanApiStarter.sln
```

Tests cover JWT claims, refresh-token generation, rotation, revocation and reuse, role constants, registration, login, authorization behavior, auditing, safe errors, correlation IDs, and health checks.

## Configuration

ASP.NET Core configuration precedence applies. Environment variables use double underscores, for example:

```text
ConnectionStrings__DefaultConnection
Jwt__Issuer
Jwt__Audience
Jwt__Secret
Jwt__ExpirationMinutes
Jwt__RefreshTokenExpirationDays
Seed__SuperAdminEmail
Seed__SuperAdminPassword
Seed__UserEmail
Seed__UserPassword
```

`.env` is ignored by Git. `.env.example` contains safe placeholders and the full local configuration surface. Real environment variables take precedence over values loaded from `.env`.

## Using this starter for a new project

1. Use the repository as a template, fork it, or clone it without its Git history.
2. Replace local development placeholders and database names.
3. Add project-specific entities and use cases within the existing dependency direction.
4. Add migrations and tests with each feature.
5. Review every item in the production checklist below.

To rename safely:

1. Replace `CleanApiStarter` in the solution, project filenames, assembly names, namespaces, Docker labels, and documentation.
2. Rename the four `src` directories and the test directory.
3. Update all `ProjectReference` paths and Dockerfile `COPY` paths.
4. Update JWT issuer/audience, database name, Compose project name, and volume name.
5. Run `dotnet restore`, `dotnet build`, `dotnet test`, and `docker compose -f docker-compose.dev.yml config`.
6. Search for the old name with `rg -i 'CleanApiStarter|clean-api-starter|clean_api_starter'`.

## Before production

- Provide secrets through a managed secret store; do not deploy committed placeholders or `.env`.
- Do not use Development or its seed credentials.
- Apply migrations as a reviewed deployment step.
- Configure HTTPS, forwarded headers, trusted proxies, CORS, and allowed hosts for the deployment.
- Decide whether Swagger should be disabled or protected.
- Configure durable log collection, alerting, audit retention, and sensitive-data policies.
- Review token lifetime, key rotation, account lifecycle, password policy, rate limiting, and brute-force protection.
- Apply least-privilege database credentials and current container image patches.
- Add project-specific integration, security, and load testing.

## Documentation

- [Architecture](docs/architecture.md)
- [Authentication](docs/authentication.md)
- [Auditing](docs/auditing.md)
- [Local development](docs/local-development.md)
- [Roadmap](docs/roadmap.md)
- [Docker development](docs/docker/development.md)
- [Docker commands](docs/docker/commands.md)
- [Docker troubleshooting](docs/docker/troubleshooting.md)

## License

Licensed under the [MIT License](LICENSE).
