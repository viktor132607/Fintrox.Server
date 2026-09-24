# Persistence

Fintrox uses PostgreSQL through Entity Framework Core and the Npgsql provider.

## Versions

- Entity Framework Core: 10.0.12
- Npgsql.EntityFrameworkCore.PostgreSQL: 10.0.3
- PostgreSQL: 18.6

## Local database

Start PostgreSQL:

```bash
docker compose up -d postgres
```

PostgreSQL 18+ uses `/var/lib/postgresql` as the Docker volume target. Do not change this back to the pre-18 `/var/lib/postgresql/data` path.

## EF tooling

Restore the repository-local EF CLI:

```bash
dotnet tool restore
```

Apply migrations:

```bash
dotnet ef database update \
  --project src/Fintrox.Infrastructure/Fintrox.Infrastructure.csproj \
  --startup-project src/Fintrox.Api/Fintrox.Api.csproj
```

Create a migration:

```bash
dotnet ef migrations add MigrationName \
  --project src/Fintrox.Infrastructure/Fintrox.Infrastructure.csproj \
  --startup-project src/Fintrox.Api/Fintrox.Api.csproj \
  --output-dir Persistence/Migrations
```

Check that the model and migration snapshot match:

```bash
dotnet ef migrations has-pending-model-changes \
  --project src/Fintrox.Infrastructure/Fintrox.Infrastructure.csproj \
  --startup-project src/Fintrox.Api/Fintrox.Api.csproj
```

## Configuration

Development has a local-only connection string in `appsettings.Development.json`.

Production must provide:

```text
ConnectionStrings__DefaultConnection
```

The application fails fast when the connection string is missing.

Migrations are deliberately not applied automatically during application startup. Schema changes are explicit deployment operations and are validated by CI.

## Schemas

The persistence baseline creates these PostgreSQL schemas:

- `core`
- `identity`
- `accounting`
- `sales`
- `purchases`
- `payments`
- `tax`
- `integration`
- `audit`

The EF migration history table remains in PostgreSQL's `public` schema. Business tables will be assigned to their module schema as each module is implemented.
