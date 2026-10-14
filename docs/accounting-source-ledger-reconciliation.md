# ACCOUNTING-001F1 — sales / purchases / payments source-to-ledger reconciliation

**2026-10-14 — IMPLEMENTED IN CODE, CI/.NET BUILD/MIGRATED POSTGRESQL EXECUTION NOT YET VERIFIED.**

This is the **first bounded slice** of `ACCOUNTING-001F`, not the completed cross-subledger financial reconciliation. It checks the source document ↔ original journal integrity of the **active legacy** Sales/Purchases/Payments APIs and one real PostgreSQL ledger; it cannot replace bank statement reconciliation, AR/AP aged balances or a close checklist.

## HTTP API

`GET /api/v1/accounting/fiscal-years/{fiscalYearId}/source-reconciliation`

Requires a valid bearer identity, active tenant membership and `accounting.read`. A year in another tenant returns **404**. The API is read-only: it **never posts, repairs, reverses or silently reconciles journal entries**.

The response `FinancialSourceReconciliationResponse` contains:
- Fiscal year ID/date range, source document count, original system journal count, issues count.
- `salesPostingIntegrityVerified`, `purchasesPostingIntegrityVerified`, `paymentsPostingIntegrityVerified`: **current source-to-original-journal consistency flags only**, not a full receivables/payables reconciliation.
- `bankingReconciliationAvailable=false` and `financialSubsystemsFullyReconciled=false`: the Banking runtime remains a scaffold, so **never mark enterprise finance fully reconciled on this evidence**.
- An ordered list of typed per-source `FinancialSourceReconciliationIssue` records, carrying `sourceType`, `sourceId`, `journalEntryId`, `code`, `detail`.

## Evidence and invariants

- Uses tenant-scoped read-only EF queries over existing `sales.invoices`, purchase documents, payments, allocations and `accounting.journal_entries` / `journal_lines`. No migration or accounting balance mutation.
- The report uses the original source business date **inside the selected fiscal year**, but evaluates each document's **current** status and existing system journal state. **It is not a historical as-of-date snapshot**, especially for documents cancelled after the year end.
- Origin references are the operational `AutoPostingEngine` contract: `sales-invoice:{Guid:N}`, `purchase-document:{Guid:N}` and `payment:{Guid:N}`.
- Each final source requires **one posted** original journal. Each Cancelled source requires the original journal to be `Reversed` with **exactly one linked Posted reversal**, whose ordered lines match the original account/line numbers and invert debit/credit exactly. A Draft source may **not** have an original posted system journal.
- Detects: `MissingPosting`, `UnexpectedPosting`, `DuplicatePosting`, `OrphanPosting`, `PostingDateMismatch`, `PostingStatusMismatch`, `MissingReversal`, `ReversalLineMismatch`, `UnbalancedPosting`, `MissingExchangeRate`, `ControlAmountMismatch`, `PaymentAllocationMismatch`, `InvalidAllocationTarget`, `UnsupportedStatus`.
- Source gross monetary value is converted using the **same** `decimal.Round(amount / exchangeRate, 4, MidpointRounding.AwayFromZero)` formula as `AutoPostingEngine.ToBase`. The expected control line is the original system-generated sales AR debit, purchase AP credit or incoming/outgoing payment asset debit/credit, identified by immutable source posting descriptions.
- Confirmed payment `AllocatedAmount` is checked against the sum of its stored **payment-currency** `PaymentAllocation.PaymentAmount` rows, plus positive amounts, payment direction and max amount checks. Document-currency amounts are not naively compared to payment amounts.
- The PostgreSQL fiscal-year `FOR UPDATE` lock serializes this report with cooperating source automatic postings and financial calendar status transitions to prevent a journal being half-posted at snapshot time. This is a single-database transaction, not a cross-application distributed lock.
- Detects orphan original system journals within the selected year even where no corresponding active source exists.

## Tests added (12; NOT EXECUTED / CI NOT VERIFIED)

Nine initial source HTTP/postgres regressions plus two exact-reversal regressions in
`FinancialHttpPostgresSourceReconciliationTests.cs`, and one sales/purchase regression in
`FinancialHttpPostgresInvoiceReconciliationTests.cs`.

The cases cover zero-source non-banking attestation, payment missing/matching/incorrect GL posting, orphan system journal, draft and cross-tenant isolation, unauthenticated access, payment allocation mismatch, exact posted reversal, balanced-but-incorrect cancellation reversal, and issued sales/received purchase with missing journal.

These are **regression code**, not a claim of green test results or an approved finance close.

## Remaining in ACCOUNTING-001F

1. **Real AR/AP open-items and aged balances**: per counterparty/document, FX carrying value, overpayment, unapplied advances, credit notes, returns, cancellation and historical cutoffs; reconcile against dedicated control account balances rather than a transient/current-rule mapping.
2. **Banking**: configure real bank-account sources, statement imports, statement-line ↔ payment matching, unmatched ledger/bank items, bank fees, payment clearing and reconciliation approvals. Its target module currently has Entity scaffolds but no operational banking persistence/API.
3. **Cash/payment-method settlement**, bank/card/cash registers, multi-currency realized/unrealized FX, independent accounting dimensions and control account change history.
4. Subsequent close-cycle evidence, independent reviewer approval and discrepancy resolution. Reports must refuse to assert `financialSubsystemsFullyReconciled` until these exist.
5. Actual CI/Release compilation, migrated PostgreSQL integration suites and security/performance checks. Confirm via `docs/ai/VALIDATION.md`.

The 936 new multi-module Domain entities remain separate target scaffolds and are not made operational by this legacy slice.
