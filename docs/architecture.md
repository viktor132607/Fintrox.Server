# Fintrox modular monolith

## Scope of this change

The target is an ASP.NET Core Web API + a separate Next.js frontend + PostgreSQL.
This change creates structural boundaries only. Existing phase 21 behavior remains in
the original horizontal projects; it has NOT been migrated into the new modules.
No financial rules, controllers, database migrations or background workers are added.
Xero/SAP are product scope references, not claims about their internal architecture.

## Physical boundaries

`src/Fintrox.Api` is the single deployable composition root.
Each `src/Modules/<Module>` has separate Domain, Application, Contracts,
Infrastructure and Presentation assemblies. Separate projects enforce dependency
direction at compile time; modules can later be packaged with their own host.

Allowed project dependencies:

| Assembly | Allowed references |
|---|---|
| Domain | None |
| Contracts | None |
| Application | Own Domain, own Contracts, explicitly approved foreign Contracts |
| Infrastructure | Own Application, Domain, Contracts; approved foreign Contracts for adapters |
| Presentation | Own Application, Contracts |
| API host | Module Infrastructure and Presentation; legacy projects temporarily |

There are currently zero cross-module references. DisableTransitiveProjectReferences
prevents accidental access through another project's dependencies.
`architecture/modules.json` inventories all boundaries, future schemas and use-case folders.
`scripts/check_architecture.py` validates the project graph, catalog, solution and host
wiring in CI; it does not claim to validate future SQL or runtime behavior.
Any future foreign Contracts dependency must be reviewed and added to the policy explicitly.
No shared business kernel, generic repository, empty service interfaces, event bus,
message broker, CQRS framework or service discovery is introduced speculatively.

## Business modules

| Module | Reserved schema | Owned capabilities | Existing code to migrate later |
|---|---|---|---|
| Identity | `identity` | Users, Sessions, Roles, Permissions | Identity, Authentication, Authorization |
| Organizations | `organizations` | Companies, Memberships, Settings, Branches | Organizations; core schema |
| Accounting | `accounting` | ChartOfAccounts, FiscalCalendar, Journals, Posting, PeriodClosing | Accounting; ledger consistency boundary |
| Counterparties | `counterparties` | Customers, Suppliers, Contacts, PaymentTerms | Counterparties; Domain/Partners; core schema |
| Sales | `sales` | Quotes, Orders, Invoices, CreditNotes, Receivables | Sales |
| Purchases | `purchases` | Orders, SupplierInvoices, Expenses, CreditNotes, Payables | Purchases |
| Payments | `payments` | Receipts, Disbursements, Allocations, Advances | Payments |
| Banking | `banking` | BankAccounts, Statements, Reconciliation, CashManagement | Not implemented |
| Tax | `tax` | VatCodes, TaxDetermination, TaxReturns, Localizations | Tax |
| Currencies | `currencies` | Currencies, ExchangeRates, Revaluation | Currencies; Domain/Tax; tax schema |
| Inventory | `inventory` | Catalog, Warehouses, StockMovements, Valuation | Not implemented |
| FixedAssets | `fixed_assets` | AssetRegister, Depreciation, Disposals | Not implemented |
| Projects | `projects` | Projects, CostCenters, TimeEntries, CostAllocation | Not implemented |
| Budgeting | `budgeting` | Budgets, Forecasts, VarianceAnalysis | Not implemented |
| Consolidation | `consolidation` | Groups, Intercompany, Eliminations, ConsolidatedStatements | Not implemented |
| Payroll | `payroll` | Employees, PayRuns, Contributions, Localizations | Not implemented; optional future module |
| Reporting | `reporting` | FinancialStatements, LedgerReports, ManagementReports, Exports | Reports; direct accounting reads require migration |
| Documents | `documents` | Attachments, Templates, Imports, Retention | Not implemented |
| Workflows | `workflows` | Approvals, Tasks, Notifications | Not implemented |
| Integrations | `integrations` | ApiClients, ExternalEvents, Idempotency, Inbox, Outbox, Webhooks | Integrations; integration schema |
| Audit | `audit` | AuditTrail, AccessHistory, Export | Audit; transaction-local auditing requires migration |

Schema names above are target ownership, not database objects created by this change.
Accounting keeps journal, chart, fiscal calendar and posting in one consistency boundary.
Sales owns receivables; Purchases owns payables; Payments owns cash settlement and allocation.
Banking owns statements/reconciliation, not payment state. Reporting owns derived read models,
not ledger writes. Integrations owns external adapters, not other modules' business rules.
Payroll and other unimplemented ERP areas are reserved scope, not promised functionality.

## PostgreSQL and tenancy

- Initially one PostgreSQL database, later one database per extracted service if needed.
- Each implemented target module owns its DbContext, schema and migration history table.
- Business data is company scoped; organization identifiers never replace server-side membership checks.
- Carry organization, actor, correlation and causation identifiers across module calls/events.
- Never trust organization headers or frontend state as authorization.
- No target cross-module FK, navigation, repository access or joins. Exchange IDs and immutable snapshots.
- Tenant-aware constraints/indexes and isolation tests are required before module activation.
- Migrations run through a controlled deployment job, not concurrently from every API replica.
- Monetary precision, currency, rounding and time rules are defined per business capability before implementation.

## Integration and extraction rules

Start with direct in-process application ports backed by approved versioned Contracts.
Do not add loopback HTTP inside the monolith. Keep network/transport decisions in adapters.
Public API DTOs and integration events live in Contracts/Api/V1 and Contracts/Events/V1;
internal domain events remain private to their module.

Before introducing asynchronous boundaries, specify eventual consistency, ordering,
idempotency, inbox/outbox, retry limits, dead letters, reconciliation and compensation.
Posting stays atomic inside Accounting. A sale and its ledger posting may need an explicit
pending/posted/failed workflow after separation; the existing atomic behavior must not be
silently weakened. Never replace a local ACID transaction with a naive publish-after-save.

To extract one module: first migrate its implementation and data ownership; eliminate
all legacy shared-context/transaction dependencies; establish contract and failure tests;
add a standalone API host; swap in-process adapters for HTTP/messages; migrate its schema
with backfill/reconciliation and rollback; switch gateway routing without changing FE imports.
The frontend uses one logical API base URL and module adapters. Gateway deployment is deferred.

## Existing coupling and migration gates

The current Fintrox.Infrastructure/Persistence/FintroxDbContext owns multiple domains;
auto-posting, allocations, reporting, audit and inbox/outbox rely on this shared context.
These are explicit legacy exceptions, not extracted modules. Existing schemas include core,
identity, accounting, sales, purchases, payments, tax, integration and audit.
Do not move migrations, rename schemas, replay initial migrations or change production data
as part of scaffolding. The existing migration chain and DbContext stay authoritative.

Future migration is one capability at a time: characterize existing behavior with tests,
introduce a contract/adapter, move ownership, verify parity, then remove the legacy path.
No new feature implementation belongs in legacy horizontal projects without an explicit decision.

## Cross-cutting concerns

Host owns authentication middleware, Problem Details, rate limiting, CORS and composition.
Authorization is also enforced at use-case boundaries. Infrastructure owns observability,
secrets/configuration, storage and transport adapters. Avoid shared mutable singleton state;
API instances should be stateless. Durable jobs and messages are partitioned by ownership.
Audit, accounting history and operational logs have distinct retention and access policies.
These are rules for implementation, not claims that all controls are present today.

## Verification

```bash
python3 scripts/check_architecture.py
dotnet restore Fintrox.Server.sln
dotnet build Fintrox.Server.sln --configuration Release --no-restore
dotnet test Fintrox.Server.sln --configuration Release --no-build
```

Existing CI still validates EF migrations and the Docker image. Reserved module test
folders will contain unit, PostgreSQL integration and public-contract tests when behavior
is introduced. No meaningless empty business tests are generated for the skeleton.

## Platform extension

The catalog now contains 23 modules: the original 21 plus Capabilities and Experience. See [activation and experience](activation-experience.md). The shared architecture/experience-integration.json manifest is design metadata only. Its reserved entries do not enable a feature, grant permission or represent a working widget.
