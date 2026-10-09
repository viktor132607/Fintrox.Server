# Repository audit — AUDIT-001 — 2026-10-09

## Scope and evidence
Server baseline: `04e50ceb7a4d050658de6b7c430fa753f21a6673`; Client: `3b49bd197212d822677eba995bdf2aa519d17cea`.
Recursively inventoried every tracked file in both checkouts (Server 1,414; Client 211), including configuration, migration/design files, documentation and all placeholder directories. Inspected source inventories, public service operations, controller routes/policies, persistence mapping patterns, dependency registration, authentication/authorization paths, transactions, webhook delivery, tests and actual frontend source.
This is a feature/architecture and risk audit, not a proof that every business path is correct. Runtime claims below are limited to observed CI; full API/database concurrency regression coverage is absent.

## Verified baseline gates
Server CI run 37860035722, job 113593691210: restore, Release build with warnings as errors, **33/33 tests**, EF model snapshot validation and migration application to PostgreSQL all succeeded.
Client CI run 37859970261 succeeded (npm ci, architecture/tool/source typechecks, Next.js build).
Original 186 projects compile. Module projects contain no business C# files. Core horizontal implementation remains active.

## Every module
PARTIAL below means existing legacy implementation, not completed target-module migration or production readiness. Every target module is still SCAFFOLD.

| Module | Existing legacy features | Evidence under src/ | Status / remaining work |
|---|---|---|---|
| Accounting | ChartOfAccounts, FiscalCalendar, Journals, Posting | Application/Accounting; Domain/Accounting; Infrastructure/Accounting | PARTIAL — Opening balances, recurring journals, accruals, allocations, complete fiscal closing, multi-book and dimensions |
| Audit | AuditTrail | Infrastructure/Audit; Persistence/FintroxDbContext.cs | PARTIAL — Dedicated query/export/retention/AccessHistory APIs and tests |
| Banking | None; planned: BankAccounts, Statements, Reconciliation, CashManagement | Modules/Banking/ | SCAFFOLD — all listed use cases unimplemented |
| Budgeting | None; planned: Budgets, Forecasts, VarianceAnalysis | Modules/Budgeting/ | SCAFFOLD — all listed use cases unimplemented |
| Capabilities | None; planned: Registry, Dependencies, ModuleActivation, CapabilityActivation, Configuration, Availability | Modules/Capabilities/ | SCAFFOLD — all listed use cases unimplemented |
| Commerce | None; planned: Catalog, Pricing, Promotions, CommerceOrders, StorefrontIntegrations, PointOfSale | Modules/Commerce/ | SCAFFOLD — all listed use cases unimplemented |
| Consolidation | None; planned: Groups, Intercompany, Eliminations, ConsolidatedStatements | Modules/Consolidation/ | SCAFFOLD — all listed use cases unimplemented |
| ContractManagement | None; planned: CustomerContracts, SupplierContracts, ContractTerms, Renewals, Obligations | Modules/ContractManagement/ | SCAFFOLD — all listed use cases unimplemented |
| Counterparties | Customers, Suppliers, Contacts, PaymentTerms | Application/Counterparties; Domain/Partners/Counterparty.cs | PARTIAL — Separate contact collections and advanced terms; API/tenant regression tests |
| CRM | None; planned: Leads, Opportunities, Activities, Customer360, SalesPipeline | Modules/CRM/ | SCAFFOLD — all listed use cases unimplemented |
| Currencies | Currencies, ExchangeRates | Application/Currencies; Domain/Accounting/Currency.cs; Domain/Accounting/ExchangeRate.cs | PARTIAL — Revaluation and comprehensive multi-currency accounting |
| Documents | None; planned: Attachments, Templates, Imports, Retention | Modules/Documents/ | SCAFFOLD — all listed use cases unimplemented |
| Experience | None; planned: Profiles, LayoutTemplates, UserLayouts, WidgetCatalog, Navigation, Preferences, ConfigurationVersions | Modules/Experience/ | SCAFFOLD — all listed use cases unimplemented |
| FixedAssets | None; planned: AssetRegister, Depreciation, Disposals | Modules/FixedAssets/ | SCAFFOLD — all listed use cases unimplemented |
| HumanResources | None; planned: Recruitment, Onboarding, EmployeeRecords, LeaveManagement, PerformanceManagement | Modules/HumanResources/ | SCAFFOLD — all listed use cases unimplemented |
| Identity | Users, Sessions, Roles, Permissions | Infrastructure/Identity/AuthenticationService.cs; Application/Authorization; Api/Controllers/AuthController.cs | PARTIAL — MFA, reset/change-password flows, email confirmation, refresh rotation race coverage |
| Integrations | ApiClients, ExternalEvents, Idempotency, Inbox, Outbox, Webhooks | Application/Integrations; Infrastructure/Integrations; Api/Integrations | PARTIAL — AppRegistry/Installations/activation/mappings/SDK are scaffolds; SSRF hardening and multi-worker delivery claim/recovery |
| Inventory | None; planned: Catalog, Warehouses, StockMovements, Valuation | Modules/Inventory/ | SCAFFOLD — all listed use cases unimplemented |
| Manufacturing | None; planned: BillOfMaterials, WorkOrders, Routings, ProductionPlanning, ProductionCosting | Modules/Manufacturing/ | SCAFFOLD — all listed use cases unimplemented |
| OrderManagement | None; planned: OrderOrchestration, Fulfillment, Backorders, Returns, OrderRouting | Modules/OrderManagement/ | SCAFFOLD — all listed use cases unimplemented |
| Organizations | Companies, Memberships, Settings | Application/Organizations; Infrastructure/Organizations | PARTIAL — Branches and settings depth; last-owner concurrency; inactive-tenant behavior |
| Payments | Receipts, Disbursements, Allocations | Application/Payments; Domain/Payments | PARTIAL — Advanced unapplied advances, banking reconciliation, payment gateway lifecycle |
| Payroll | None; planned: Employees, PayRuns, Contributions, Localizations | Modules/Payroll/ | SCAFFOLD — all listed use cases unimplemented |
| Projects | None; planned: Projects, CostCenters, TimeEntries, CostAllocation | Modules/Projects/ | SCAFFOLD — all listed use cases unimplemented |
| Purchases | SupplierInvoices, Expenses, Payables | Application/Purchases; Domain/Purchases; Application/Payments | PARTIAL — Purchase orders, approvals, goods receipts and dedicated supplier credit-note flow |
| QualityManagement | None; planned: Inspections, QualityControl, Nonconformities, CorrectiveActions, Traceability | Modules/QualityManagement/ | SCAFFOLD — all listed use cases unimplemented |
| Reporting | LedgerReports | Application/Reports; Infrastructure/Reports | PARTIAL — P&L/balance sheet/cash flow, custom reports, saved searches, export and read-model isolation |
| ResourceManagement | None; planned: ResourceScheduling, CapacityPlanning, Utilization, ResourceAllocation, Timesheets | Modules/ResourceManagement/ | SCAFFOLD — all listed use cases unimplemented |
| RevenueManagement | None; planned: RecognitionRules, RevenueSchedules, DeferredRevenue, RevenueContracts, Adjustments | Modules/RevenueManagement/ | SCAFFOLD — all listed use cases unimplemented |
| Sales | Invoices, Receivables | Application/Sales; Domain/Sales; Application/Payments | PARTIAL — Quotes, sales orders, dedicated credit-note flow, aging and collection workflows |
| ServiceManagement | None; planned: SupportTickets, ServiceLevels, FieldService, Maintenance, CustomerSupport | Modules/ServiceManagement/ | SCAFFOLD — all listed use cases unimplemented |
| SubscriptionBilling | None; planned: Plans, Subscriptions, BillingCycles, UsageBilling, Renewals, Proration | Modules/SubscriptionBilling/ | SCAFFOLD — all listed use cases unimplemented |
| SupplyChain | None; planned: DemandPlanning, SupplyPlanning, ProcurementPlanning, Replenishment, SupplierCollaboration | Modules/SupplyChain/ | SCAFFOLD — all listed use cases unimplemented |
| Tax | VatCodes | Application/Tax; Domain/Tax | PARTIAL — Tax returns, jurisdiction/localization validation, complete tax determination |
| WarehouseManagement | None; planned: Receiving, Putaway, Picking, Packing, Shipping, CycleCounting | Modules/WarehouseManagement/ | SCAFFOLD — all listed use cases unimplemented |
| Workflows | None; planned: Approvals, Tasks, Notifications | Modules/Workflows/ | SCAFFOLD — all listed use cases unimplemented |

## Backend layers and data
- Models/value objects: existing entities under Domain/{Accounting,Organizations,Partners,Sales,Purchases,Payments,Tax,Integrations}; new boundaries contain placeholders only. No generic shared-kernel migration performed.
- Application: account/fiscal/journal/posting, counterparties, currency/VAT, invoices, purchase documents, payments, reports, organizations and integration services are registered in Application/DependencyInjection.cs.
- Presentation/contracts: 22 legacy controllers expose versioned routes; transport request/response models live under Contracts. API auth policy is registered in Program.cs. No new-module endpoint is registered.
- Validation: request annotations and explicit domain/application guards exist; no basis for claiming every use case has complete validation.
- Persistence: shared FintroxDbContext owns all implemented domains. Organization-scoped queries, composite tenant-aware financial foreign keys, indexes/uniqueness and explicit decimal precision exist. ValidateOrganizationScopes checks nonempty IDs; it is not a global tenant filter.
- Transactions/posting: accounting transaction runner has retries and nested savepoints; journal services validate open periods/year, account constraints and balanced posting; posting/reversal database protections are present. No broad regression suite verifies them.
- Migrations: existing chain and snapshot pass CI on a clean PostgreSQL instance; upgrade/rollback/production migration testing is not established.
- Tests: only six original domain test files / 33 test cases; no original service/controller, HTTP, tenant-isolation or PostgreSQL concurrency test suite. Placeholder module test directories are not tests.
- Background work: webhook worker has retries/failure records/signatures, but no atomic multi-worker claim/lease and crash recovery; explicit hardening task required.

## Frontend
Next.js App Router + strict TypeScript. Actual UI is page.tsx (development placeholder) and layout.tsx; 23 module index.ts files export empty boundaries. No business screens, login flow, typed operational API client, forms, data fetching, state handling or responsive design system is implemented. No Tailwind dependency is installed. Simple/accountant/expert, widgets and integration areas are placeholders. Client has 23-module manifests; Server has 36, so alignment is a queued architecture task. No current working business UI exists to refactor.

## Infrastructure and operations
Docker multi-stage image and PostgreSQL compose configuration exist. CI builds/tests/migrations and container publication are configured. /health/live and /health/ready are mapped. JWT secret is required; baseline configuration uses placeholders. No CORS/rate-limiting middleware is registered in Program.cs; no production backup/recovery, distributed tracing, load testing or deployment verification is established by this audit. Existing database audit is not a substitute for security/access logs.

## Prioritized concrete findings
| ID | Priority | Evidence | Action |
|---|---|---|---|
| AUTH-001 | P0 | Program.cs validates JWT signature/lifetime only; PermissionAuthorizationHandler trusts integration claims | Revalidate active user/client/tenant and current scopes on every bearer request; selected for immediate repair |
| AUTH-002 | P0 | AuthenticationService.RefreshAsync reads active token then updates it; RefreshTokenConfiguration has no concurrency guard | Make refresh consumption atomic and add relational race regression tests |
| INT-001 | P0 | WebhookSubscription.NormalizeUrl accepts any HTTP(S); worker sends with default redirects | Add SSRF-safe destination/DNS/redirect policy and adversarial tests before exposing webhook administration |
| ORG-001 | P1 | Last-owner count and mutation are separate operations | Serialize owner removal/demotion and test simultaneous operations |
| TEST-001 | P1 | Only six baseline domain test files | Add accounting invariant, HTTP authorization, cross-tenant and PostgreSQL transaction/concurrency suites |
| INT-002 | P1 | Worker loads due deliveries and marks attempts without atomic lease | Claim/lease/recovery and multi-worker tests |
| API-001 | P1 | Program.cs lacks CORS/rate limiting | Add explicit configured frontend origins and endpoint throttling before frontend auth rollout |
| FE-001 | P2 | Client manifests/shells stop at 23 modules | Align 36 module shells/manifests, then implement typed UI incrementally |

No claim of complete security or accounting compliance. AUTH-001 is a targeted fix in the currently active legacy authentication path; target module migration remains separate.
