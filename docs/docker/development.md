# Docker development

`docker-compose.dev.yml` is exclusively for local development. It is not hardened, scaled, or configured for production deployment.

## Services

- `postgres` uses `postgres:18-alpine`, exposes container port 5432 through `POSTGRES_PORT`, and mounts the `clean_api_starter_postgres_data` named volume at PostgreSQL 18's `/var/lib/postgresql` data root.
- `api` is built from the root multi-stage Dockerfile, exposes container port 8080 through `API_HTTP_PORT`, and starts after PostgreSQL reports healthy.
- The API receives a container-only connection string whose host is the Compose service name `postgres`. The host-side connection string in `.env.example` uses `localhost` instead.

## Start

```bash
cp .env.example .env
docker compose -f docker-compose.dev.yml up --build -d
docker compose -f docker-compose.dev.yml ps
docker compose -f docker-compose.dev.yml logs -f api
```

With default ports, open <http://localhost:8080/swagger>. Log in with either documented development account and use Swagger's `Authorize` button.

## Migrations

Because the API runs as `Development`, its startup applies pending migrations and idempotent seeds inside the Compose network. Confirm completion in API logs.

To apply migrations from the host instead, leave PostgreSQL running and execute:

```bash
dotnet tool restore
dotnet ef database update \
  --project src/CleanApiStarter.Infrastructure \
  --startup-project src/CleanApiStarter.Api
```

The runtime API image deliberately does not include the SDK or EF tool. For a manual migration entirely through Docker, run the API service and let its Development initializer apply migrations; do not extend this behavior to production.

## Rebuild and reset

```bash
docker compose -f docker-compose.dev.yml up --build -d api
docker compose -f docker-compose.dev.yml down -v
docker compose -f docker-compose.dev.yml up --build -d
```

`down -v` permanently deletes the local PostgreSQL volume. Use it only when local data may be discarded.
