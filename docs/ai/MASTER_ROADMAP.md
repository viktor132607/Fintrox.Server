# Master roadmap

**Cross-cutting financial goal:** Fintrox must be the system of record for *all* enterprise financial streams (GL, AP/AR, payments, banking, tax, payroll, inventory valuation, fixed assets, projects, budgeting, revenue recognition, consolidation and reporting). Modules stay separately owned; external apps must integrate through secure versioned contracts and must not maintain a second authoritative financial ledger. See [Financial System of Record](../financial-system-of-record.md). Implement and test by phases; this requirement is not satisfied by Entity scaffolds alone.


Phase 0 COMPLETE: scaffold plus successful CI restore/build/33 tests/EF snapshot/PostgreSQL migrations.
Phase 1 active: AUDIT-001, AUTH-001, AUTH-002, ORG-002 and INT-001 completed and validated. ENTITIES-EXPANSION completed across 36 modules (936 Domain models). ORG-001 completed and verified (real PostgreSQL locking/concurrency, CI 38428892598); TEST-001 completed and verified (7 full-stack HTTP and PostgreSQL finance scenarios, CI 38437997153 with 2,902 passing); INT-002 completed and CI verified (4 real PostgreSQL lease/recovery tests; 2,906 passing, run 38475619708); API-001 and platform hardening remain pending. Phases 2–10 pending; see AUDIT.md for existing legacy functionality.
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
