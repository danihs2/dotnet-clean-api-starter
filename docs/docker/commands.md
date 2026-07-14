# Docker commands

All commands run from the repository root.

## Environment lifecycle

```bash
# Start PostgreSQL and API
docker compose -f docker-compose.dev.yml up -d

# Start and rebuild images
docker compose -f docker-compose.dev.yml up --build -d

# Stop and remove containers, preserving database data
docker compose -f docker-compose.dev.yml down

# Show service state
docker compose -f docker-compose.dev.yml ps
```

## Logs

```bash
docker compose -f docker-compose.dev.yml logs -f
docker compose -f docker-compose.dev.yml logs -f api
docker compose -f docker-compose.dev.yml logs -f postgres
```

## Shells and database access

```bash
docker compose -f docker-compose.dev.yml exec api sh
docker compose -f docker-compose.dev.yml exec postgres \
  psql -U "${POSTGRES_USER:-postgres}" -d "${POSTGRES_DB:-clean_api_starter}"
```

## Database and migrations

```bash
# Permanently reset the local database volume
docker compose -f docker-compose.dev.yml down -v
docker compose -f docker-compose.dev.yml up -d postgres api

# Apply from the host while the database container runs
dotnet ef database update \
  --project src/CleanApiStarter.Infrastructure \
  --startup-project src/CleanApiStarter.Api
```

## Tests

```bash
dotnet test CleanApiStarter.sln

# Or use a temporary SDK container
docker run --rm -v "$PWD:/src" -w /src \
  mcr.microsoft.com/dotnet/sdk:10.0 dotnet test CleanApiStarter.sln
```
