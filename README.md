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

Dependency rules and layer responsibilities are documented in [docs/architecture.md](docs/architecture.md).

## Runtime

- .NET 10 / C# 14
- ASP.NET Core Web API
- Entity Framework Core 10
- Npgsql / PostgreSQL 18
- OpenAPI
- Problem Details
- Health checks

## Persistence

Local PostgreSQL:

```bash
docker compose up -d postgres
dotnet tool restore
dotnet ef database update \
  --project src/Fintrox.Infrastructure/Fintrox.Infrastructure.csproj \
  --startup-project src/Fintrox.Api/Fintrox.Api.csproj
```

Persistence and migration conventions are documented in [docs/persistence.md](docs/persistence.md).

## Run locally

```bash
dotnet restore Fintrox.Server.sln
dotnet run --project src/Fintrox.Api/Fintrox.Api.csproj
```

## Endpoints

- `GET /api/v1/system/info`
- `GET /health/live`
- `GET /health/ready`
- `GET /openapi/v1.json` in Development

`/health/ready` verifies PostgreSQL connectivity; `/health/live` does not depend on external infrastructure.

## Container

```bash
docker build -t fintrox.server .
docker run --rm -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=fintrox;Username=fintrox;Password=fintrox_dev" \
  fintrox.server
```

## CI/CD

GitHub Actions restores, builds, tests, validates EF migrations against PostgreSQL and validates the Docker image. Successful pushes to `main` publish images to:

```text
ghcr.io/viktor132607/fintrox.server
```

See [docs/ci-cd.md](docs/ci-cd.md).

Next core implementation phase: organization / multi-tenant foundation.
