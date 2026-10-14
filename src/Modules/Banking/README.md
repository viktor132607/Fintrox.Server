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
