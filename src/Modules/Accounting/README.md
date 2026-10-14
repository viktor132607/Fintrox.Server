# Accounting

Status: PARTIAL — Domain entity foundation implemented; persistence, application use cases and API pending.

Owns: ChartOfAccounts, FiscalCalendar, Journals, Posting, PeriodClosing.
Reserved PostgreSQL schema: `accounting` (not created).
Existing implementation/reference: Accounting; ledger consistency boundary.

## Boundaries

- Domain: pure model, value objects, invariants and internal domain events.
- Application: feature-oriented use cases; ports in Abstractions. Commands/queries belong under their feature when implemented.
- Contracts: versioned DTOs and integration events; no entities, EF, ASP.NET or implementation dependencies.
- Infrastructure: module-owned context, configurations, migrations, messaging and external adapters.
- Presentation: thin HTTP endpoints and transport mapping; no persistence access.
- Composition: the host explicitly wires Infrastructure and Presentation when implementation is authorized.

Foreign data is referenced by identifiers/snapshots and accessed through approved Contracts.
No cross-module DbContext, repositories, entity navigations, SQL joins, foreign keys or shared transactions in the target architecture.
See [architecture](../../../docs/architecture.md) for the current legacy exceptions and migration gates.

## Implemented entity foundation

`AccountGroup`, `AccountingDimension`, `AccountingDimensionValue`, `AccountingBook`, `RecurringJournal`.

Models live in this module Domain/Entities directory; foreign objects are referenced by IDs. These are domain types, not registered database tables or endpoints. Existing legacy workflows remain in core projects. See [module progress](../../../docs/ai/MODULE_STATUS.md) for inventory and remaining work.

## Domain expansion — 2026-10-13

Added 30 explicit, immutable, constructor-validated entity types (35 module types total): `AccountSegment`, `AccountSegmentValue`, `AccountHierarchyNode`, `AccountBalanceSnapshot`, `LedgerBookBalance`, `JournalBatch`, `JournalBatchLine`, `JournalApproval`, `PostingRuleVersion`, `PostingRuleAllocation`, `FiscalYearConfiguration`, `FiscalPeriodSchedule`, `PeriodCloseChecklist`, `PeriodCloseTask`, `PeriodCloseRun`, `ClosingEntry`, `OpeningBalanceImport`, `OpeningBalanceLine`, `AccrualSchedule`, `AccrualScheduleLine`, `DeferralSchedule`, `DeferralScheduleLine`, `CostAllocationRule`, `CostAllocationTarget`, `AccountingReconciliation`, `AccountingReconciliationLine`, `RecurringJournalOccurrence`, `DocumentNumberSequence`, `AccountingPolicyVersion`, `LedgerAdjustmentRequest`.

These are model definitions only. Existing core ledger and posting engine remain authoritative; no EF configuration, API, migrations, security rules or financial accounting logic has been added. See [module progress](../../../docs/ai/MODULE_STATUS.md).

## Financial system-of-record ownership — 2026-10-13

Fintrox manages the complete organization's financial flow. **Accounting** alone owns the authoritative GL, journal posting, reversals, accounting books and fiscal closing. Sales, Purchases, Payments, Banking, Payroll, Inventory, Commerce etc. own their operational source records and invoke approved accounting contracts; none may write journal lines into this module's tables. The existing legacy posting engine remains authoritative until parity-tested cutover. Architecture acceptance matrix: [financial system of record](../../../docs/financial-system-of-record.md).

## 2026-10-13 — TEST-001 full-stack financial regression baseline

The **existing legacy Accounting runtime** is now protected by 7 HTTP integration tests running the real API/JWT/tenant-permission flow against isolated temporary PostgreSQL databases with **real EF migrations and journal triggers**. Tests verify balanced posting and numbering, unbalanced denial, reversal and duplicate reversal prevention, closed-period posting denial, cross-company access rejection, 401/403 and posted-journal line immutability at SQL level. CI **38437997153**: 2,902 passing, 0 skipped/failed, Release build 0 warnings/errors and EF/Postgres checks passed. Fixed 44 incorrect positional request record validation-attribute targets across API contracts uncovered by HTTP tests. Details: [financial regression tests](../../../docs/financial-http-postgres-regression.md).

The Accounting module's own Infrastructure/Application/Presentation assemblies remain skeletons; this is a tested existing runtime, **not** a completed migration of the new 35 Domain Entities.

## 2026-10-14 — ACCOUNTING-001A, journal posting/closing concurrency

Implemented in the **existing legacy runtime** (`Fintrox.Application/Accounting`,
`Fintrox.Infrastructure/Accounting`): tenant-scoped PostgreSQL exclusive journal
row locks and shared fiscal-year/accounting-period row locks inside posting
transactions. Manual and source-generated postings now validate the fiscal
calendar after waiting on concurrent close, and competing post/reversal
requests for the same original are serialized. Four new real HTTP + migrated
PostgreSQL regression cases cover simultaneous posting, simultaneous
reversal, period close and fiscal-year soft-close races.

**Status: implemented, CI unverified.** No new module EF mappings, migrations,
services or endpoints are claimed. Full Accounting migration and remaining
opening-balance/closing/accrual features are still pending. See
[posting concurrency](../../../docs/accounting-posting-concurrency.md) and
[validation](../../../docs/ai/VALIDATION.md).

## 2026-10-14 — ACCOUNTING-001B, initial general-ledger balances

The active **legacy** GL now implements `POST /api/v1/accounting/opening-balances/import` (both `accounting.write` and `accounting.post`). It creates a single balanced, atomic fiscal-year starting journal, validates active balance-sheet accounts, locks the year against concurrent posting, and uses a unique system external reference to return the original journal for identical retries. Eight migrated PostgreSQL HTTP tests were added. **CI/build/tests unverified**. The 35 new Accounting module Domain entities are still **not** connected to EF persistence, services or endpoints. Detailed limitations and next work: [opening balance specification](../../../docs/accounting-opening-balances.md).

## 2026-10-14 — ACCOUNTING-001C, transactional fiscal close controls

In the **operational legacy Accounting runtime**, fiscal period/year `SoftClose`, `Close`, and `Reopen` now use tenant-scoped PostgreSQL transaction locks. Closing requires no unresolved drafts; a period must also have no misdated journals. New and edited manual journal drafts cannot bypass closed periods. Fiscal-year and period edits also use year/period locks and refuse to invalidate existing journal dates. All six close/reopen routes now require both `accounting.write` and `accounting.post`. Ten additional real HTTP/PostgreSQL integration regressions were written; **CI and test run pending**. This is **not** completed statutory annual financial closing: P&L transfer, closing journals, subledger reconciliations, approval checklist and all 35 new Accounting entity persistence integrations remain to be implemented. [Close controls and limitations](../../../docs/accounting-period-close-controls.md).

## 2026-10-14 — ACCOUNTING-001D, annual profit/loss transfer to equity

In the **active legacy** Accounting runtime, the newly authorized `POST /api/v1/accounting/fiscal-years/{fiscalYearId}/close-profit-loss` accepts an explicit retained-earnings **Equity** account. It aggregates the existing posted/reversed year's Revenue and Expense balances, creates a system-posted balanced closing journal, and atomically closes the last period and fiscal year under a PostgreSQL year lock. Prior periods must be Closed, calendar coverage contiguous, all drafts resolved. Unique System reference makes duplicate submissions safe. Direct year Close refuses outstanding P&L balances. Eight new real HTTP + migrated PostgreSQL regressions written, **CI unverified**. This does **not** map any of the new module Domain entities or implement full statutory/financial reconciliation sign-off. See [year-end accounting](../../../docs/accounting-year-end-profit-loss.md).

## 2026-10-14 — ACCOUNTING-001E, controlled correction and limited GL reconciliation

The operational **legacy** accounting runtime now has `POST /api/v1/accounting/fiscal-years/{id}/reopen-profit-loss`, requiring both accounting.write and accounting.post, the exact closing journal ID and a correction reason. It atomically posts a numbered compensating reversal, marks the original closing journal Reversed, and reopens the final period/year. Direct status-only reopen and generic reversal cannot bypass this ledger correction; later close rounds preserve immutable history under unique versioned System references. The read-only `GET .../close-reconciliation` reports annual GL debit/credit totals, profit/loss balances, draft count and fiscal readiness (GL-only, **not** statutory signed subledger reconciliation). Six new migrated PostgreSQL HTTP tests written, CI unverified. **Target Accounting module Domain scaffolds remain not wired to operational EF/API.** See [corrections](../../../docs/accounting-year-end-corrections.md).

## 2026-10-14 — ACCOUNTING-001F1, source-to-ledger reconciliation

In the **active legacy** Accounting runtime, `GET /api/v1/accounting/fiscal-years/{fiscalYearId}/source-reconciliation` is an authorized, tenant-scoped, read-only source-to-GL report for issued Sales invoices, received Purchases and confirmed Payments, including cancelled document reversal and draft journal anomalies, base-currency gross/control-line amounts and payment allocation consistency. Detects missing/duplicate/orphan system journal references and invalid reversal line/account amounts. 11 real migrated PostgreSQL HTTP tests added, **not executed/CI unverified**. Banking remains a scaffold; AR/AP historical open-item aging, bank statement matching and statutory reconciliations are **NOT** implemented, and the API explicitly reports that enterprise finance is not fully reconciled. Target Accounting module Domain entities remain scaffolded rather than mapped to EF. See [source-GL reconciliation](../../../docs/accounting-source-ledger-reconciliation.md).
