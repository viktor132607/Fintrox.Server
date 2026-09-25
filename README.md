# Fintrox.Server

Central accounting platform backend for Fintrox.

## Architecture

The backend targets .NET 10 LTS and uses Clean Architecture inside a modular monolith.

```text
src/
├── Fintrox.Api/
├── Fintrox.Application/
├── Fintrox.Contracts/
├── Fintrox.Domain/
└── Fintrox.Infrastructure/
```

See [docs/architecture.md](docs/architecture.md).

## Runtime

- .NET 10 / C# 14
- ASP.NET Core Web API
- ASP.NET Core Identity
- JWT bearer authentication
- Entity Framework Core 10
- Npgsql / PostgreSQL 18
- OpenAPI
- Problem Details
- health checks

## Authentication and authorization

Implemented:

- user registration and login;
- short-lived JWT access tokens;
- rotating refresh tokens stored as hashes;
- session listing and revocation;
- account lockout;
- organization-scoped roles;
- permission policies;
- organization member management;
- automatic Owner membership when an organization is created.

See [docs/authentication.md](docs/authentication.md).

## Multi-tenancy

Fintrox is organization-scoped. Business requests use `X-Organization-Id`, while authorization verifies the authenticated user's membership and permissions.

See [docs/multi-tenancy.md](docs/multi-tenancy.md).

## Common domain and auditing

Business entities now share common entity/auditing base classes.

Auditable business changes are written automatically to append-only `audit.audit_log` records in the same transaction as the underlying change.

See [docs/auditing.md](docs/auditing.md).

## Persistence

```bash
docker compose up -d postgres
dotnet tool restore
dotnet ef database update \
  --project src/Fintrox.Infrastructure/Fintrox.Infrastructure.csproj \
  --startup-project src/Fintrox.Api/Fintrox.Api.csproj
```

See [docs/persistence.md](docs/persistence.md).

## Run locally

```bash
dotnet restore Fintrox.Server.sln
dotnet run --project src/Fintrox.Api/Fintrox.Api.csproj
```

## Health

- `GET /health/live`
- `GET /health/ready`

## CI/CD

GitHub Actions validates build, tests, EF model/migrations against PostgreSQL and the Docker image before publishing to GHCR.

See [docs/ci-cd.md](docs/ci-cd.md).

## Chart of Accounts

Organization-scoped account hierarchy, account types, activation rules and accounting read/write authorization are implemented.

See [docs/chart-of-accounts.md](docs/chart-of-accounts.md).

## Fiscal calendar

Organization-scoped fiscal years and accounting periods are implemented with explicit `Open / SoftClosed / Closed` lifecycle rules and overlap protection.

See [docs/fiscal-calendar.md](docs/fiscal-calendar.md).

Next core phase: double-entry Journal.
