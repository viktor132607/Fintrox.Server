# Fintrox.Server

Central accounting platform backend for Fintrox.

## Current architecture

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
- OpenAPI
- Problem Details
- Health checks

## Endpoints

- `GET /api/v1/system/info`
- `GET /health/live`
- `GET /health/ready`
- `GET /openapi/v1.json` in Development

## Run locally

```bash
dotnet restore Fintrox.Server.sln
dotnet run --project src/Fintrox.Api/Fintrox.Api.csproj
```

Next phase: PostgreSQL + EF Core persistence and migrations.
