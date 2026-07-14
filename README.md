# CleanApiStarter

A reusable ASP.NET Core Web API starter built with Clean Architecture, JWT authentication, role-based access control, audit logging, Swagger/OpenAPI, health checks and PostgreSQL support.

This project is intentionally generic. It is not tied to any business domain and can be used as the foundation for multiple backend projects.

## Goals

CleanApiStarter provides a simple but professional backend foundation with:

* Clean Architecture structure.
* JWT authentication.
* Role-based access control.
* Default `SuperAdmin` and `User` roles.
* Audit logging.
* Centralized exception handling.
* Structured logging.
* Swagger/OpenAPI with JWT support.
* Health check endpoints.
* PostgreSQL support.
* Docker Compose for local development.
* Basic tests.
* Public-repository friendly documentation.

## Project Structure

```text
src/
  CleanApiStarter.Api
  CleanApiStarter.Application
  CleanApiStarter.Domain
  CleanApiStarter.Infrastructure

tests/
  CleanApiStarter.Tests

docs/
  architecture.md
  authentication.md
  auditing.md
  local-development.md
  roadmap.md
```

## Architecture

The project uses a lightweight Clean Architecture approach.

### Api

Responsible for:

* Controllers/endpoints.
* Middleware registration.
* Authentication and authorization setup.
* Swagger configuration.
* Health checks.
* Dependency injection composition root.

### Application

Responsible for:

* Use cases.
* DTOs.
* Application services.
* Interfaces.
* Validation.
* Authentication contracts.
* Audit contracts.

### Domain

Responsible for:

* Core entities.
* Enums.
* Base auditable entity.
* User and role models.
* Audit log model.
* Generic domain rules.

### Infrastructure

Responsible for:

* EF Core DbContext.
* PostgreSQL integration.
* Entity configurations.
* JWT token generation.
* Password hashing.
* Audit persistence.
* Database seeders.
* Infrastructure services.

## Default Roles

The starter includes two default roles:

### SuperAdmin

Full access to all protected endpoints.

### User

Normal authenticated user access.

Projects that use this starter can add their own roles later.

## Authentication Endpoints

Suggested endpoints:

```http
POST /api/auth/login
GET /api/auth/me
```

Optional development endpoint:

```http
POST /api/auth/register
```

## Demo Authorization Endpoints

```http
GET /api/demo/public
GET /api/demo/authenticated
GET /api/demo/super-admin
```

Expected behavior:

* `/api/demo/public` is accessible without authentication.
* `/api/demo/authenticated` requires a valid JWT.
* `/api/demo/super-admin` requires the `SuperAdmin` role.

## Health Endpoints

```http
GET /health
GET /api/health
```

The health check should validate that the API is running. When PostgreSQL is configured, it should also validate database connectivity.

## Audit Logging

The starter includes a reusable audit module for tracking important system events.

Audit events may include:

* Login success.
* Login failure.
* User created.
* Role assigned.
* Protected action executed.
* Process failure.
* Unexpected exception.

Suggested audit fields:

```text
Id
EventType
EntityName
EntityId
UserId
UserEmail
Role
IpAddress
UserAgent
Message
MetadataJson
CreatedAtUtc
Success
ErrorCode
ErrorMessage
```

The audit module should be reusable across future projects.

## Swagger

Swagger/OpenAPI should be available in development.

The Swagger UI should include:

* JWT bearer authorization.
* Tags grouped by module:

  * Auth
  * Users
  * Audit
  * Demo
  * Health
* Collapsed operations by default to keep the UI clean.

## Local Development

### Requirements

* .NET SDK.
* Docker.
* Docker Compose.
* PostgreSQL client tools, optional.

### Run PostgreSQL locally

```bash
docker compose up -d
```

### Apply migrations

```bash
dotnet ef database update --project src/CleanApiStarter.Infrastructure --startup-project src/CleanApiStarter.Api
```

### Run the API

```bash
dotnet run --project src/CleanApiStarter.Api
```

### Run tests

```bash
dotnet test
```

## Configuration

Use environment variables or user secrets for sensitive values.

Example settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=clean_api_starter;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Issuer": "CleanApiStarter",
    "Audience": "CleanApiStarter",
    "Secret": "CHANGE_THIS_SECRET_FOR_DEVELOPMENT_ONLY",
    "ExpirationMinutes": 60
  }
}
```

Never commit production secrets.

## Default Development User

For local development, the project may seed a default SuperAdmin user.

Example:

```text
Email: admin@example.com
Password: ChangeMe123!
Role: SuperAdmin
```

These credentials are for local development only and must be changed before production.

## Production Checklist

Before using this starter in production:

* Change JWT secret.
* Disable or protect development seed users.
* Configure secure database credentials.
* Configure CORS properly.
* Review logging level.
* Review audit retention policy.
* Configure HTTPS.
* Configure deployment environment variables.
* Review Swagger exposure.
* Add project-specific roles and permissions.
* Add project-specific business modules.

## How to Use This Starter for a New Project

1. Clone or fork this repository.
2. Rename the solution and projects.
3. Update namespaces.
4. Update database name.
5. Update JWT issuer and audience.
6. Add project-specific roles.
7. Add business modules inside the clean architecture structure.
8. Keep authentication, audit, health checks and error handling as shared foundation.

## Roadmap

Possible future improvements:

* Refresh tokens.
* Email confirmation.
* Password reset.
* Permission-based authorization.
* Multi-tenancy.
* Outbox pattern.
* Background jobs.
* Docker production profile.
* CI pipeline.
* API versioning.
* Rate limiting.
