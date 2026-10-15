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

## 2026-10-14 — ACCOUNTING-001F2, open-items vs AR/AP control accounts

Operational **legacy** Accounting now exposes `GET /api/v1/accounting/fiscal-years/{id}/open-item-reconciliation` (accounting.read, tenant/year scoped). It computes every active Sales/Purchase document's open original-currency/base-currency carrying value net of Confirmed PaymentAllocation rows, with immutable original document FX rate and 4dp rounding; returns document aging, per-counterparty balances and cumulative posted/reversed GL control balances from all years through the fiscal-year-end cutoff. Detects unmatched GL control movements, overallocations, wrong payment counterparties/directions, missing control configurations/source journals and allocation header/row mismatch; includes former account mappings plus source journal control lines. 11 real migrated PostgreSQL HTTP test cases written but **not run / CI unverified**. Historical-as-of account aging and bank statement reconciliation are unavailable, so full-finance reconciliation flags are false and `ACCOUNTING-001F` remains in progress. New Accounting Domain scaffolds remain unmapped. See [AR/AP reconciliation scope](../../../docs/accounting-open-item-reconciliation.md).

## 2026-10-14 — ACCOUNTING-001F3, first operational bank reconciliation

In active legacy Accounting/Banking, backed by new PostgreSQL migration, account setup binds one base-currency bank account to one dedicated operational Asset GL account; immutable statement import and explicit payment matching require exact tenant-owned Confirmed BankTransfer plus Posted original System journal GL line. Bank statement reconciliation returns statement balance, posted cumulative GL bank-account balance, outstanding bank/book items and signed differences. Nine full HTTP + migrated PostgreSQL regressions written, **not yet executed / CI unverified**. The target Banking Domain entity scaffolds still await EF integration. This is not group/legal financial reconciliation; see [bank MVP](../../../docs/accounting-bank-statement-reconciliation.md).

## 2026-10-14 — ACCOUNTING-001F4A, bank CSV and deterministic matching

The operational legacy Banking layer now accepts bounded RFC4180 CSV and offers tenant-scoped match suggestions and a protected exact 1:1 auto-match for confirmed base-currency BankTransfers verified against their Posted original GL journal. It reuses the existing database lock/unique match constraints and does **not** post new journal lines. Five migrated PostgreSQL HTTP and four parser unit scenarios written, CI unverified. The fees/FX/bank close remainder of F4 is pending; no company-wide signed finance reconciliation claim. [Scope](../../../docs/accounting-bank-csv-auto-match.md).

## 2026-10-14 — ACCOUNTING-001F4B, approved fee financial effects

Active legacy GL now supports bounded bank-fee posting from immutable negative statement evidence, booking DR analytical Expense / CR dedicated bank Asset in an open fiscal period, with one System journal and unique immutable source link. Replays, tenant access, concurrent close and double matching guarded; six migrated PostgreSQL HTTP tests authored but not executed. Tax, multi-currency fees and controlled bank fee correction remain pending. [Bank fee specification](../../../docs/accounting-bank-fee-posting.md).

## 2026-10-14 — ACCOUNTING-001F4C1, realized foreign bank exchange result

The operational legacy GL now accepts bank-initiated realized FX settlement postings from explicitly authorized foreign statement lines with approved tenant-dated ExchangeRate snapshots and original AR/AP carrying bases; it posts balanced bank Asset/clearing/FX gain or loss journal entries within the open fiscal year/period. New bank FX audit linkage immutable; original GL number retained and duplicate post blocked. Native foreign bank amount can never be subtracted from base-currency GL balance to claim fully reconciled banking. Six HTTP/PostgreSQL tests written but CI unverified; unrealized FX revaluation and end-to-end signed financial close still pending. [FX details](../../../docs/accounting-foreign-bank-settlements.md).

## 2026-10-14 — ACCOUNTING-001F4C2, period-end unrealized FX

Operational GL accepts a bounded controlled foreign bank Asset revaluation only at open period cutoff, with complete native zero-opening bank movement history, traceable posted original settlement GL and selected cutoff dated currency rate. Its approved action updates bank asset base carrying value to `Round(nativeClose / quoteRate,4,AwayFromZero)`, via numbered Posted Revenue/Expense System gain/loss journal and immutable unique valuation evidence; zero difference records evidence without dummy GL. Nine full HTTP/PostgreSQL tests authored, not executed. Formal close signoff, native/base bridge, controlled corrections and multi-currency liability valuation are TODO. [Specification](../../../docs/accounting-foreign-bank-revaluation.md).

## 2026-10-14 — ACCOUNTING-001F4C3A FX bank evidence and independent review

Active legacy Accounting validates the bridge from native foreign bank statement to revalued base-currency bank Asset GL rather than subtracting unmatched units. It checks full native source chain, original Posted FX settlement entries, cutoff rate snapshot, numbered revaluation journal and lack of extra/reversed bank GL. A maker and a different `organizations.manage` approver persist immutable note/fingerprint review evidence, with replay and stale evidence protections. Five migrated PostgreSQL HTTP tests written but not executed; no full statutory signoff or controlled fee/FX correction implementation. [Scope](../../../docs/accounting-foreign-bank-close-review.md).

## 2026-10-14 — ACCOUNTING-001F4C3B1 protected bank GL compensations

Approved bank fee or an FX settlement not included in a cutoff revaluation can now be fully reversed via a guarded source-linked correction endpoint. A numbered compensating System journal exactly swaps all original debits/credits; original ledger status becomes Reversed; immutable audit link and actor/reason stored with it transactionally under bank/period/journal locks. The old bank evidence is not modified, and report treats the source as unresolved rather than cleanly matched. Generic re-reversal of protected bank compensation refused. Eight real migrated PostgreSQL HTTP tests authored but unverified; F4C3B2 valued/approved cutoff correction and enterprise finance close not delivered. [Scope](../../../docs/accounting-bank-posting-corrections.md).

## 2026-10-14 — ACCOUNTING-001F4C3B2 guarded valuation journal replacements

The active legacy GL now supports audited, numbered correction of an original foreign bank cutoff valuation (not correction of historical source transactions) through a tenant-approved, version-preconditioned banking route. Previous vN System valuation posted journal is exactly reversed under bank/period locks, the original marked Reversed, a new vN+1 balanced bank Asset/FX gain/loss System journal posted, and unique immutable audit evidence committed together; zero-delta versions generate audit only. New maker/checker approval for versioned native/base bank GL reconciliation; original history stays immutable. Nine real PostgreSQL HTTP tests written, CI unverified. Formal multi-period statutory reopening and full finance source correction remain pending. [Details](../../../docs/accounting-versioned-fx-bank-corrections.md).
\n2026-10-15 — ACCOUNTING-001G1 CODED (CI PENDING): legacy Accounting added immutable tenant-scoped monthly recurring journal templates (fixed 1–28 day, optional final date, 2–100 balanced debit/credit lines), authorized GET/POST templates, secured one-per-date manual generate route with template FOR UPDATE lock and normal GL fiscal period locks, normalized append-only PostgreSQL evidence, current account revalidation, System Draft+lines inserted atomically, separate authorized GL post and protection against manual mutation of the generated draft. Five actual migrated-PostgreSQL HTTP regression methods authored, NOT VERIFIED YET. See docs/accounting-recurring-journals.md; ACCOUNTING-001G2 scheduler/revisions and 001H accruals pending; no automatic posting.\n