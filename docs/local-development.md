# Local development

## Prerequisites

- .NET 10 SDK.
- PostgreSQL, or Docker with Compose.
- Optional PostgreSQL client tools.

Restore tools and packages:

```bash
dotnet tool restore
dotnet restore CleanApiStarter.sln
```

## PostgreSQL through Docker

```bash
cp .env.example .env
docker compose -f docker-compose.dev.yml up -d postgres
docker compose -f docker-compose.dev.yml ps
```

Run the API on <http://localhost:8080>:

```bash
dotnet run --project src/CleanApiStarter.Api --launch-profile http
```

Development startup applies the initial migration, creates roles, and creates both documented accounts. Swagger is at <http://localhost:8080/swagger>.

## Configuration overrides

Compose automatically reads `.env`. The API also loads the root `.env` when running in `Development`, without replacing variables already set by the operating system. Connection strings and secrets are intentionally not committed in appsettings. In other environments, `.env` is not loaded.

Use user secrets for private local values:

```bash
dotnet user-secrets init --project src/CleanApiStarter.Api
dotnet user-secrets set --project src/CleanApiStarter.Api \
  'Jwt:Secret' 'a-private-local-secret-at-least-32-characters-long'
```

## Migrations

```bash
dotnet ef migrations list \
  --project src/CleanApiStarter.Infrastructure \
  --startup-project src/CleanApiStarter.Api

dotnet ef database update \
  --project src/CleanApiStarter.Infrastructure \
  --startup-project src/CleanApiStarter.Api
```

Set `ConnectionStrings__DefaultConnection` before the command when targeting a non-default local database.

## Validation

```bash
dotnet build CleanApiStarter.sln
dotnet test CleanApiStarter.sln
docker compose -f docker-compose.dev.yml config
```
