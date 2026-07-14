# Docker troubleshooting

## Port already in use

Change `API_HTTP_PORT` or `POSTGRES_PORT` in `.env`, then recreate the services. Check listeners with `docker ps` and the operating system's port tools.

## PostgreSQL connection failed

Run `docker compose -f docker-compose.dev.yml ps` and inspect PostgreSQL logs. Inside Compose, the API host must be `postgres` and port `5432`; from the host, use `localhost` and the exposed `POSTGRES_PORT`. Recreate both services after changing database credentials so their values remain consistent.

## Migrations not applied

Confirm `ASPNETCORE_ENVIRONMENT=Development`, inspect API logs, and verify the database health check. Development startup applies migrations. From the host, run the documented `dotnet ef database update` command with the host connection string.

## JWT secret missing or invalid

The API exits during startup when issuer, audience, or a secret of at least 32 characters is missing. Copy `.env.example` to `.env`, set a local value, and recreate the API container. Do not use the example value outside local development.

## API container exits immediately

```bash
docker compose -f docker-compose.dev.yml logs api
docker compose -f docker-compose.dev.yml config
```

Typical causes are invalid configuration, PostgreSQL credentials that no longer match an existing volume, a migration error, or a build failure.

## Swagger not reachable

Swagger is enabled only in Development. Confirm the API is running, `ASPNETCORE_ENVIRONMENT` is `Development`, and browse to `http://localhost:<API_HTTP_PORT>/swagger`. Check that no local firewall or another process owns the port.

## Docker volume still has old data

Changing `POSTGRES_PASSWORD` does not update an already initialized database volume. If the data is disposable, reset it:

```bash
docker compose -f docker-compose.dev.yml down -v
docker compose -f docker-compose.dev.yml up --build -d
```

This deletes all local database data.
