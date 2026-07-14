# Repository Guidelines

## Project Structure & Module Organization

The solution follows lightweight Clean Architecture:

- `src/CleanApiStarter.Domain`: entities, role constants, and domain rules. Keep it framework-independent.
- `src/CleanApiStarter.Application`: use cases, DTOs, validation, exceptions, and interfaces. It may depend only on Domain.
- `src/CleanApiStarter.Infrastructure`: EF Core, PostgreSQL, repositories, JWT/password implementations, migrations, auditing, and seed logic.
- `src/CleanApiStarter.Api`: controllers, middleware, authorization, Swagger, health checks, and dependency composition.
- `tests/CleanApiStarter.Tests`: xUnit unit and API integration tests.
- `docs/`: architecture, authentication, auditing, local setup, and Docker guidance.

Add generic starter capabilities only; do not introduce project-specific business concepts.

## Build, Test, and Development Commands

```bash
dotnet tool restore                         # Restore the local EF Core tool
dotnet restore CleanApiStarter.sln          # Restore NuGet dependencies
dotnet build CleanApiStarter.sln            # Compile all projects
dotnet test CleanApiStarter.sln             # Run unit and integration tests
dotnet format CleanApiStarter.sln           # Apply repository formatting
dotnet run --project src/CleanApiStarter.Api --launch-profile http
```

For the complete development environment:

```bash
cp .env.example .env
docker compose -f docker-compose.dev.yml up --build -d
```

Swagger is available at `http://localhost:8080/swagger`.

## Coding Style & Naming Conventions

Follow `.editorconfig`: UTF-8, LF endings, final newlines, four-space indentation for C#, and two spaces for JSON, YAML, and Markdown. Nullable references and implicit usings are enabled. Prefer file-scoped namespaces and primary constructors where they improve clarity.

Use PascalCase for types and public members, camelCase for parameters and locals, and `I` prefixes for interfaces. Async methods should end in `Async`. Keep provider-specific code in Infrastructure and HTTP concerns in API.

Run `dotnet format CleanApiStarter.sln --verify-no-changes` before submitting.

## Testing Guidelines

Tests use xUnit and `WebApplicationFactory`. Name tests by expected behavior, for example `User_can_access_authenticated_but_not_super_admin_endpoint`. Add unit tests for isolated rules and integration tests for HTTP, persistence, authentication, or authorization changes. New behavior should cover success, validation, and access-control paths.

## Commit & Pull Request Guidelines

History currently contains only `first commit`, so no established convention exists. Use short imperative commit subjects such as `Add refresh token validation`, and keep unrelated changes separate.

Pull requests should explain the change, motivation, configuration or migration impact, and verification commands. Link relevant issues. Include screenshots only for Swagger/UI changes and never commit `.env`, credentials, tokens, or production connection strings.

## Security & Configuration

Treat committed values as local placeholders. Store private values in environment variables, user secrets, or a managed secret store. Development seed accounts and automatic migrations must remain restricted to `Development`.
