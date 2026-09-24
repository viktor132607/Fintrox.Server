# Fintrox.Server

Central accounting platform backend for Fintrox.

## Current state

Phase 2 is complete: the repository contains a clean ASP.NET Core Web API scaffold targeting .NET 10 LTS.

### Runtime

- .NET 10 / C# 14
- ASP.NET Core Web API
- OpenAPI
- Problem Details
- Health checks

### Endpoints

- `GET /api/v1/system/info`
- `GET /health/live`
- `GET /health/ready`
- `GET /openapi/v1.json` in Development

## Run locally

```bash
dotnet restore Fintrox.Server.sln
dotnet run --project src/Fintrox.Api/Fintrox.Api.csproj
```

The next phase introduces the full Clean Architecture / modular-monolith project structure before PostgreSQL and EF Core are added.
