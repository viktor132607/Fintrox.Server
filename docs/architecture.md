# Fintrox architecture

Fintrox.Server is a modular monolith built around Clean Architecture boundaries.

## Dependency rule

```text
Fintrox.Domain       <- Fintrox.Application <- Fintrox.Infrastructure
       ^                        ^
       |                        |
Fintrox.Contracts ----------- Fintrox.Api
                                  |
                                  +-> Fintrox.Infrastructure
```

Actual project references:

- `Fintrox.Domain`: no project dependencies.
- `Fintrox.Contracts`: no project dependencies.
- `Fintrox.Application`: references Domain and Contracts.
- `Fintrox.Infrastructure`: references Application and Domain.
- `Fintrox.Api`: references Application, Infrastructure and Contracts.

## Responsibilities

### Domain

Pure business model and accounting invariants. It must not reference ASP.NET Core, Entity Framework Core, PostgreSQL, HTTP, authentication providers or UI code.

### Application

Use cases, orchestration, commands, queries, application-level validation and abstractions required by use cases.

### Infrastructure

Implementations for persistence, identity, external systems, integrations, audit storage and other technical concerns.

### Contracts

Stable API request/response contracts and integration contracts. Contracts must not expose EF entities.

### Api

HTTP transport only: routing, authentication/authorization, middleware, OpenAPI and mapping HTTP input/output to application use cases.

## Module direction

Business features will be grouped by module inside these layers instead of creating a separate service for every feature. Planned modules include Accounting, Organizations, Counterparties, Sales, Purchases, Payments, Tax, Reports and Integrations.
