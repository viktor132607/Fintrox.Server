# Banking

Status: PARTIAL — Domain entity foundation implemented; persistence, application use cases and API pending.

Owns: BankAccounts, Statements, Reconciliation, CashManagement.
Reserved PostgreSQL schema: `banking` (not created).
Existing implementation/reference: Not implemented.

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

`BankAccount`, `BankStatement`, `BankStatementLine`, `BankReconciliationMatch`.

Models live in this module Domain/Entities directory; foreign objects are referenced by IDs. These are domain types, not registered database tables or endpoints. Existing legacy workflows remain in core projects. See [module progress](../../../docs/ai/MODULE_STATUS.md) for inventory and remaining work.

## Domain expansion — 2026-10-13

Added 22 constructor-validated entity records (26 total): `BankBranch`, `BankFeedConnection`, `BankFeedSyncJob`, `BankStatementImport`, `BankStatementImportError`, `BankStatementBalance`, `BankTransactionCategory`, `BankTransactionClassification`, `BankReconciliationSession`, `BankReconciliationAdjustment`, `BankPaymentInstruction`, `BankPaymentBatch`, `BankPaymentBatchLine`, `BankMandate`, `BankTransferBeneficiary`, `CashAccount`, `CashRegister`, `CashMovement`, `CashForecast`, `BankFeeRule`, `BankFeeCharge`, `CashSweepRule`.

Model definitions only; existing legacy runtime is unchanged. Persistence, services, DTOs, endpoints, authorization and relational tests are pending. See [module status](../../../docs/ai/MODULE_STATUS.md).

## 2026-10-14 — ACCOUNTING-001F3 bounded operational Banking MVP

**Runtime location:** Active legacy API/Infrastructure, **not** the as-yet unmapped 36-module Banking Domain entity scaffolds. Explicit migration `20261014000100_BankStatementReconciliation` creates actual `banking.bank_accounts`, immutable `bank_statements`/`bank_statement_lines` with tenant FKs and unique payment matches. `/api/v1/banking` offers bank accounts, signed and idempotent statement import, verified one-to-one bank transfer match to posted System GL journal and base-currency bank-vs-ledger read-only reconciliation. `IBankReconciliationService` uses the same EF connection for parameterized Npgsql transactions. Nine migrated PostgreSQL HTTP tests written, **CI/unittest run pending**. No external bank feeds, FX, bank fees, status approval, auto-matching, or full enterprise finance reconciled attestation. Details: [Operational Banking F3](../../../docs/accounting-bank-statement-reconciliation.md).

## 2026-10-14 — ACCOUNTING-001F4A, CSV import / exact auto matching

In the active legacy operational Banking API (not the unwired target module Domain assembly), a bounded Fintrox four-column UTF-8 CSV import now uses the same immutable transactional statement path as F3, including SHA256 exact replay. A separate read-only match suggestion endpoint finds only unambiguous one-to-one pairs of bank line and tenant Confirmed BankTransfer with exact signed amount, value/payment date, currency and Posted original System journal on this bank's GL Asset account. Protected `auto-match` reruns the existing validated line match for each pair, reporting skipped races/ambiguities; no guessed posting. Five new full migrated PostgreSQL HTTP tests and four parser unit tests written, **not executed**. Bank fee expense journaling, bank FX, true bank feed parsers and signed closing approval are pending. See [automation details](../../../docs/accounting-bank-csv-auto-match.md).

## 2026-10-14 — ACCOUNTING-001F4B, bank fee expense journal

In operational legacy Banking (target Banking Domain remains scaffolded), authorized `POST /api/v1/banking/statements/{statementId}/lines/{lineId}/post-fee` creates an actual balanced Posted System GL journal DR chosen tenant-owned analytical Expense / CR dedicated bank Asset. It requires original negative imported line, matching amount, reason, open fiscal period, locks and unique immutable bank fee↔journal evidence. Replays safe; payment matches and automatic suggestions exclude fee lines. Bank reconciliation separately counts `postedFeeLines` only for valid Posted bank-credit evidence. Six HTTP + migrated PostgreSQL tests written, CI unverified. No FX, VAT automation or controlled fee correction. [Fee design](../../../docs/accounting-bank-fee-posting.md).

## 2026-10-14 — ACCOUNTING-001F4C1, explicit foreign-currency bank settlement

**Operational legacy Banking**, not new unmapped target module, now supports tenant-configured active foreign bank currency. A dated foreign statement line can be explicitly settled against an approved stored FX rate (quote units per base unit), original base-currency AR/AP clearing carrying value and configured gain/loss GL accounts through `POST .../post-foreign-settlement`. It posts a numbered balanced System journal and immutable one-to-one rate and amount evidence atomically with fiscal locks, replay protection and account type validation. Foreign statement details display `foreignSettlementJournalEntryId`, and report counts verified posted foreign settlement lines, **but refuses to compare native closing directly with base GL** (null difference, full reconciliation false). Six migrated PostgreSQL HTTP tests written, NOT EXECUTED. FX revaluation and native closing certification remain TODO. [Implementation detail](../../../docs/accounting-foreign-bank-settlements.md).

## 2026-10-14 — ACCOUNTING-001F4C2, unrealized FX asset bank revaluation

In active legacy Banking, signed period-end native statements and exact-date selected ExchangeRate can generate a posted System GL valuation of an existing foreign **Asset** bank balance (DR bank/CR FX Revenue for uplift; DR FX Expense/CR bank for shortfall). Only after verifying zero-opening contiguous bank history, all original Posted FX-settlement GL references, no unrecognized bank GL, open calendar and expected cumulative base balance. PostgreSQL migration adds immutable one-bank-period audit evidence with zero-adjustment support; protects against duplicate/post-cutoff backdating and generic reversal. Nine full migrated PostgreSQL HTTP tests added, not executed. No foreign bank overdraft/liability valuation or native-vs-base certified reconciled flag until F4C3. See [F4C2 safeguards](../../../docs/accounting-foreign-bank-revaluation.md).

## 2026-10-14 — ACCOUNTING-001F4C3A internal foreign-bank closing controls

The **active legacy Banking runtime**, not unmapped target Domain scaffolds, now exposes `GET /api/v1/banking/statements/{id}/foreign-close-evidence` (native statement chain + posted FX system sources + immutable cutoff revaluation + GL asset base balance after revaluation). Distinct authorized maker `prepare-foreign-close` and checker `approve-foreign-close` persist separate immutable tenant audit rows and source fingerprint under bank+calendar locks; stale fingerprints, self-approval and unaudited movements conflict. 5 real migrated PostgreSQL HTTP tests written but NOT RUN. This is internal review only: no certified external bank signature, no legal financial close, and no approved correction/reversal workflow; F4C3B pending. See [internal bank close controls](../../../docs/accounting-foreign-bank-close-review.md).

## 2026-10-14 — ACCOUNTING-001F4C3B1 guarded bank fee and unvalued FX correction

The operational legacy Banking API now has `POST /api/v1/banking/statements/{id}/lines/{lineId}/correct-posting` (accounting.write/post), a real append-only 1-to-1 GL compensation workflow for an exact original BankFee or ForeignSettlement System journal. It locks bank account/line, open correction fiscal calendar and original journal; creates a numbered immutable inverse journal, marks original Reversed and stores source+reversal+reason+actor evidence atomically. Identical retries replay; mismatched tenant/GL and late correction of a previously revalued/prepared FX bank cutoff conflict. Imported bank evidence remains unchanged and original bank line correctly becomes unmatched requiring further approved bookkeeping; generic GL reverse of compensation forbidden. 8 full migrated PostgreSQL HTTP tests written but NOT RUN. Versioned approved FX revaluation/closing correction is F4C3B2, not done. [Details](../../../docs/accounting-bank-posting-corrections.md).
