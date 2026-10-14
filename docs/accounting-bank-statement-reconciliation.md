# ACCOUNTING-001F3 — operational base-currency bank statement reconciliation

**2026-10-14 — IMPLEMENTED IN CODE, BUILD/CI/MIGRATED POSTGRESQL EXECUTION NOT VERIFIED.**

This is the third bounded slice of ACCOUNTING-001F, and the **first actual persistent banking runtime** in the existing Fintrox API. The target 36-module Banking Domain entities are **still scaffolded and are not automatically substituted for the operational legacy runtime**. A PostgreSQL migration creates operational tables and a real accounting/banking service; no fake in-memory statement store.

## Database and migration

`src/Fintrox.Infrastructure/Persistence/Migrations/20261014000100_BankStatementReconciliation.cs`

The hand-authored migration is discoverable using standard `[DbContext]` and `[Migration]` attributes. It uses **raw PostgreSQL DDL**, outside the EF entity model (therefore no generated snapshot changes). Future migrations must preserve its schema and not attempt to recreate these SQL tables.

- `banking.bank_accounts`: tenant, normalized IBAN, name, base currency, **dedicated** active analytical bank GL Asset account; tenant-unique IBAN and tenant-unique GL account.
- `banking.bank_statements`: tenant, account, external statement ID, start/end dates, signed opening and closing balances, canonical SHA-256 import fingerprint, import timestamp; uniqueness by tenant/account/external ID, prevents overlapping date windows at API level.
- `banking.bank_statement_lines`: tenant, statement, external line reference, value date, **signed** amount (positive receipt, negative payment), description, optional matched payment and match timestamp. Composite tenant foreign keys; tenant-wide unique matched payment; check constraints on nonzero amounts and coherent match state.
- PostgreSQL triggers make imported statement headers **immutable**; statement-line evidence (date/reference/currency amount/description/tenant) cannot be changed or deleted, and a matched payment cannot be replaced/removed. Only setting an initial verified match is allowed.
- Transactions serialize imports and matches on a tenant bank-account row (`FOR UPDATE`); statement lines are locked for matching. The cross-worker unique index prevents matching the same confirmed payment to multiple bank lines even through different bank accounts.
- Bank funds and imported statement rows do **not** create automatic accounting entries. Matching is read-only against the already-posted payment's GL journal, except the bank match record.

## New authenticated endpoints

All paths begin `/api/v1/banking`:
- **GET `/accounts`** — list the current tenant's bank accounts, requiring `accounting.read`.
- **GET `/statements/{statementId}`** — view imported statement, all line IDs, signed amounts and current matched payment IDs, requiring `accounting.read`.
- **POST `/accounts`** — `CreateBankAccountRequest` (`iban`, `name`, `currencyCode`, `ledgerAccountId`). Requires `accounting.write` AND `accounting.post`. Account currency must equal the tenant organization's base currency; GL destination must be active, analytical and type `Asset`. Returns 201.
- **POST `/accounts/{accountId}/statements`** — `ImportBankStatementRequest` (`externalId`, `fromDate`, `toDate`, signed `openingBalance`/`closingBalance`, and one or more lines `externalId`, `valueDate`, `signedAmount`, `description`). Max 10,000 lines/import; exactly four decimal places; no duplicate line references, zero amounts, out-of-period lines, or arithmetic discrepancies. Exact replay returns **200** with unchanged statement ID and `alreadyImported=true`; changed replay returns **409**; new import returns **201**. Dates must not overlap with another statement for that bank account.
- **POST `/statements/{statementId}/lines/{lineId}/match`** — `MatchBankStatementLineRequest` containing `paymentId`. Requires both accounting permissions. The payment must belong to the active tenant, be `Confirmed`, use `BankTransfer`, have base-currency code and exchange rate 1, its sign/amount must equal the statement line, and payment and posted journal dates must not exceed the statement end date. A posted System journal `payment:{paymentId:N}` must contain **one exact bank GL account debit-minus-credit line** equal to the signed bank movement. Another payment already on that line -> 409; the same match request replays successfully; the same payment matched on any other line -> 409 due to unique DB constraint. Nothing is silently allocated to a different company/payment.
- **GET `/statements/{statementId}/reconciliation`** — requires `accounting.read`. A PostgreSQL repeatable-read snapshot computes bank opening + signed line totals, imported bank closing, cumulative posted/reversed GL balance on the mapped bank account through statement cutoff, bank-minus-GL difference, matched and unmatched statement lines, confirmed posted book bank payments still uncleared by bank evidence, and `fullyReconciled`. It rechecks **current payment status**, currency, amount, date and original posted GL entry for existing matches: later cancellation invalidates a formerly matched line in this report. No GL posting occurs.

## Test coverage authored but NOT verified

9 additional full HTTP + disposable migrated PostgreSQL integration regressions:
1. Successful import -> confirmed bank payment -> original posted system GL journal -> match -> exact bank/GL close and identical match replay.
2. Identical statement replay keeps the original ID, while changed contents reject with 409.
3. Bad opening/closing arithmetic and repeated line references reject before persistence.
4. Wrong bank statement amount is rejected for matching, and unmatched bank + uncleared book entries expose the signed difference.
5. Another tenant cannot import to the bank account or read/match its statements.
6. Bank account cannot point to Revenue GL account or use non-base currency.
7. The same payment cannot be assigned to two separate imported statements.
8. A previously matched payment cancelled afterward is no longer a valid match for reporting.\n9. Bank account listing and statement line detail expose only the current tenant's records.

**No successful .NET compilation, PostgreSQL test run, or production rollout is claimed.** Tests and CI must be executed and recorded in `docs/ai/VALIDATION.md`.

## Deliberate constraints and remaining work

- **Base-currency account only.** No multi-currency bank account, intra-day FX settlement, bank conversion, realized/unrealized revaluation, cash pooling, SWIFT/SEPA/ISO20022/camt.053 or PSD2 bank feed connector.
- The import accepts an already **parsed structured statement**, not direct CSV/MT940/OFX/PDF ingestion; provider-specific parsing, file signatures, ISO reference normalization, line duplication across different statement IDs and statement-chain balance continuity checks are separate work.
- Matching is manual by an explicitly supplied `paymentId`; no probabilistic auto-match, bank fees, payment creation from statement line, chargebacks, many-to-one matches or split settlements.
- No formal reconciliation session sign-off, approver evidence, amendment workflow or bank statement reconciliation cutoff watermark. `FullyReconciled` means only a specific statement is internally matched and agrees to **its dedicated base-currency GL account**; it is not enterprise finance/statuory reconciliation.
- Bank-account IBAN is normalized and checked for length/characters but **not yet ISO-13616 checksum-validated**. Production import should add checksum and bank statement file identity verification.
- Source GL and multi-year AR/AP open-item reporting still have their independent constraints (historical current-status vs event-time and future outstanding balances).
- `ACCOUNTING-001F` remains in progress, and `financialSubsystemsFullyReconciled=false` continues in the fiscal finance reports until real finance-wide closing evidence is available.
