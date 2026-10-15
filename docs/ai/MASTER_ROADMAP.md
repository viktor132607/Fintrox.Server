# Master roadmap

**Cross-cutting financial goal:** Fintrox must be the system of record for *all* enterprise financial streams (GL, AP/AR, payments, banking, tax, payroll, inventory valuation, fixed assets, projects, budgeting, revenue recognition, consolidation and reporting). Modules stay separately owned; external apps must integrate through secure versioned contracts and must not maintain a second authoritative financial ledger. See [Financial System of Record](../financial-system-of-record.md). Implement and test by phases; this requirement is not satisfied by Entity scaffolds alone.


Phase 0 COMPLETE: scaffold plus successful CI restore/build/33 tests/EF snapshot/PostgreSQL migrations.
Phase 1 active: AUDIT-001, AUTH-001, AUTH-002, ORG-002 and INT-001 completed and validated. ENTITIES-EXPANSION completed across 36 modules (936 Domain models). ORG-001 completed and verified (real PostgreSQL locking/concurrency, CI 38428892598); TEST-001 completed and verified (7 full-stack HTTP and PostgreSQL finance scenarios, CI 38437997153 with 2,902 passing); INT-002 completed and CI verified (4 real PostgreSQL lease/recovery tests; 2,906 passing, run 38475619708); API-001 and platform hardening remain pending. Phase 2 **active**: legacy Accounting hardening 001A-C, annual P&L journal 001D and controlled correction/GL reconciliation 001E and source-to-GL integrity 001F1 (Sales/Purchases/Payments) plus 001F2 (document/partner AR/AP open-item versus GL controls) coded; operational base-currency Banking statement import/payment matching 001F3 and bounded CSV/exact one-to-one auto-match 001F4A and approved bank-fee GL posting 001F4B and first realized foreign bank FX settlement 001F4C1 and guarded period-end unrealized bank FX valuation 001F4C2 coded; historical AR/AP snapshots, external bank feeds, 001F4C3A bounded native/base reconciliation and independent two-user internal review coded; 001F4C3B1 controlled fee/unvalued realized FX compensating corrections coded; 001F4C3B2 versioned historical bank FX cutoff correction and independent new-version maker/checker review coded; coordinated multi-period restatement, bank authenticity and legal closing signoff still pending, CI verified on source SHA e28a16a11c695cdec191448f4d353f9f7435c089 (GitHub #503: 3031/3031, EF/Postgres/Docker); future changes need re-verification; remaining Accounting/Tax/Currencies and statutory closing tasks are pending. Phases 3–10 pending; see AUDIT.md for existing legacy functionality.
The user authorized continuation after the model switch. Detailed business specification: MASTER_PROMPT.md.

След архитектурното разширение изпълнявай следните фази.

## PHASE 0 — Architecture Expansion

- 13 new modules
- 65 new projects
- .slnx registration
- README files
- Project memory
- Validation
- Commit

**ТАЗИ ФАЗА Е ПЪРВА И ЗАДЪЛЖИТЕЛНА.**

## PHASE 1 — Foundation Stabilization

- Build
- Tests
- Dependencies
- Database Validation
- Identity
- Organizations
- Security
- Module Boundaries

## PHASE 2 — Accounting Foundation

- Accounting
- Posting Engine
- Chart of Accounts
- Fiscal Periods
- Counterparties
- Currencies
- Tax

## PHASE 3 — Financial Transactions

- Sales
- Purchases
- Payments
- Banking
- Receivables
- Payables
- Automatic Posting

## PHASE 4 — Operational ERP

- Inventory
- Fixed Assets
- Projects
- Budgeting
- Payroll

## PHASE 5 — Financial Control

- Reporting
- Consolidation
- Documents
- Workflows
- Audit

## PHASE 6 — Extensibility

- Capabilities
- Module Activation
- Experience Engine
- Integrations
- App Registry
- SDK
- Custom Layout Builder

Надграждай вече съществуващите реализации.

## PHASE 7 — New ERP Modules

Разработвай функционалностите на:

- CRM
- OrderManagement
- WarehouseManagement
- SupplyChain
- Manufacturing
- RevenueManagement
- SubscriptionBilling
- HumanResources
- QualityManagement
- ServiceManagement
- Commerce
- ResourceManagement
- ContractManagement

Редът трябва да отчита действителните зависимости.

## PHASE 8 — Full Frontend

- Simplified Interface
- Accountant Interface
- Expert Interface
- Module Screens
- Dashboards
- Administration
- Experience Builder
- Integrations Management

## PHASE 9 — Enterprise Extensions

- Multi-book Accounting
- Advanced Revenue Recognition
- Saved Searches
- Custom Records
- Custom Fields
- Advanced Manufacturing
- Subscription Management
- Customer/Vendor Portals
- Advanced Analytics
- AI-assisted Workflows

## PHASE 10 — Production Hardening

- Security
- Testing
- Performance
- Observability
- Localization Validation
- Backup/Recovery
- CI/CD
- Documentation
- Deployment Readiness

Не започвай всички фази едновременно.

Работи на малки, проверими milestones.

---
\n2026-10-15 — ACCOUNTING-001G1 CI VERIFIED on source SHA a7544869081888e7c090c9abba32919cb3316e4b: GitHub CI/CD #522 (38658049296): Release dotnet build --warnaserror SUCCESS, 3036/3036 tests passed with 0 failures and 0 skips, EF model snapshot clean, migration 20261015000100_RecurringJournalTemplates applied and listed on PostgreSQL 18.6, Docker Build & Publish SUCCESS. Added immutable monthly recurring journal templates and tenant-scoped one-per-schedule-date System Draft generation, with explicit separate authorized GL posting. No automatic scheduling. ACCOUNTING-001G2 (template revisions, scheduling/monitoring) and 001H accruals/deferrals pending. Whole ACCOUNTING-001 remains IN PROGRESS.\n