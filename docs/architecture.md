# Architecture

CleanApiStarter uses a small Clean Architecture dependency graph:

```text
Api -> Application <- Infrastructure
          |
        Domain

Infrastructure -> Domain
Tests -> all projects
```

## Domain

`CleanApiStarter.Domain` contains framework-independent entities and constants: `User`, `Role`, `AuditLog`, `AuditableEntity`, and the default role names. It has no project references.

## Application

`CleanApiStarter.Application` owns use cases, request/response models, validation rules, repository contracts, authentication contracts, audit contracts, and application exceptions. It references Domain but not EF Core or ASP.NET Core.

Repository interfaces are use-case focused. There is intentionally no generic repository, mediator, command bus, or separate mapping framework.

## Infrastructure

`CleanApiStarter.Infrastructure` implements Application contracts. It contains:

- `AppDbContext` and EF Core configurations.
- PostgreSQL provider registration and migrations.
- User, role, and audit repositories.
- Password hashing and JWT generation.
- Cryptographically secure refresh-token generation, hashing, rotation, and persistence.
- Audit persistence.
- Development database initialization and seed data.

Changing database providers should be isolated here: replace provider registration, provider package, design-time factory, and migrations. Domain and Application must remain provider-independent.

## API

`CleanApiStarter.Api` is the composition root and HTTP adapter. Controllers translate HTTP requests into application service calls. Middleware and configuration handle errors, correlation, authentication, authorization, health, Swagger, and logging. Controllers do not access EF Core directly.

## Request flow

1. Correlation middleware accepts or creates `X-Correlation-ID`.
2. Exception middleware protects the pipeline.
3. JWT authentication establishes identity claims.
4. Authorization policies enforce authenticated or SuperAdmin access.
5. A controller invokes an Application service.
6. Infrastructure fulfills persistence or token contracts.
7. The controller returns the common API envelope.

## Deliberate boundaries

The starter includes one role per user, JWT access tokens, rotating refresh tokens, and synchronous audit persistence. It does not include fine-grained permissions, background processing, distributed messaging, or tenancy isolation. Add such capabilities only when a real project requires them.
