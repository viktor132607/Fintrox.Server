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

Dependency rules are documented in [docs/architecture.md](docs/architecture.md).

## Runtime

- .NET 10 / C# 14
- ASP.NET Core Web API
- Entity Framework Core 10
- Npgsql / PostgreSQL 18
- OpenAPI
- Problem Details
- health checks

## Multi-tenancy

Fintrox is organization-scoped. Organization management and the `X-Organization-Id` request context are implemented.

See [docs/multi-tenancy.md](docs/multi-tenancy.md).

Authentication/membership authorization is the next phase.

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
