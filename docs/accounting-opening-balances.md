# ACCOUNTING-001B — initial general-ledger balances

**Implemented in the active legacy Accounting runtime on 2026-10-14; CI / PostgreSQL execution not yet verified.** This is a bounded phase-2 capability, **not** the completed Accounting module, a full multi-book conversion, or an operational migration of the 35 new Accounting Domain models.

## API contract

`POST /api/v1/accounting/opening-balances/import`

Requires a valid active organization context and **both** `accounting.write` and `accounting.post` server-side authorization policies.

Example payload:

```json
{
  "fiscalYearId": "00000000-0000-0000-0000-000000000001",
  "lines": [
    {
      "accountId": "00000000-0000-0000-0000-000000000002",
      "debit": 1500.0000,
      "credit": 0,
      "description": "Cash opening balance"
    },
    {
      "accountId": "00000000-0000-0000-0000-000000000003",
      "debit": 0,
      "credit": 1500.0000,
      "description": "Owner equity"
    }
  ]
}
```

HTTP 200 returns the existing `JournalEntryResponse` (including `Posted` status,
journal number, dates, lines, accounting period, external reference and created/updated audit fields).
An identical retry returns the same journal ID/number; a changed retry returns 409.
Invalid business state/tenant/account/fiscal-calendar consistency returns 409.
ASP.NET contract invalidity returns 400; missing authentication and required permissions
are handled by the existing 401/403 authorization pipeline.

## Rules

1. The opening position belongs to **exactly one organization and fiscal year**.
   It is posted on the fiscal year's **start date**, which must fall in an
   existing **open** accounting period of that fiscal year. The year must also be open.
2. Between 2 and 1,000 lines, with unique active posting accounts, positive
   amount on exactly one side of each line, and at most 4 fractional digits.
   Total debits and credits must match exactly and be greater than zero.
3. Accept only accounts belonging to the requesting tenant that are active,
   manually postable **balance-sheet** types (Asset, Liability, Equity).
   Revenue/Expense/OffBalance and foreign-tenant accounts are rejected.
   Subledger control accounts excluded from manual posting are not bypassed.
4. An organization/fiscal year cannot import an opening position if it already
   has **any posted or reversed journal activity** in that fiscal year.
   A draft alone does not count as a posted financial effect.
5. `IJournalRepository.LockFiscalYearForOpeningBalancesAsync` obtains
   PostgreSQL `SELECT ... FOR UPDATE` on the tenant-specific fiscal-year row
   within the transaction. Ordinary manual/automatic posting acquires a
   `FOR SHARE` lock on the same year, so an import is serialized with other
   same-year financial posting commits. Fiscal status is re-read after locking.
6. The single authoritative Accounting journal is created first in
   **Draft**, its lines persisted, a posting number allocated, then the journal
   moved to **Posted** in the same transaction. A failed validation or SQL operation
   rolls back the entire entry, lines and number allocation.
7. The current deployed schema's unique system-source external-reference index
   gives a durable per-tenant idempotency identity:
   `opening-balance:v1:<fiscal-year-guid-N>` with
   `JournalEntrySource.System`. A replay uses the same source reference and
   compares persisted status, date and line contents before returning the
   original journal. This avoids a new migration and preserves the existing GL.
8. Later corrections must use authorized reversals and controlled new
   fiscal-opening workflows; simply replaying changed lines is not allowed.
   The opening import is not an external app general-purpose posting endpoint.

## Tests added (CI verification pending)

Eight HTTP/real migrated-PostgreSQL regression tests added under
`tests/Fintrox.Domain.Tests/Accounting/FinancialHttpPostgresRegressionTests.cs`:

- Balanced posted opening and identical idempotent retry.
- Unbalanced or foreign-tenant account rejection with zero sequence impact.
- Rejection after an already-posted manual journal.
- Rejection of a changed repeat after successful import.
- Tenant scoping and anonymous authorization rejection.
- Competing identical import requests return the same journal and consume one sequence.
- Revenue/expense-type opening account rejection.
- Closed first fiscal period and soft-closed fiscal year rejection.

## Deliberate boundaries

- Supports a **single base-currency GL opening position** per fiscal year, not
  detailed accounts receivable/payable aging, bank reconciliation, inventory
  layers, customer/vendor subledger opening, historic per-document migration,
  comparative-year balances, multi-book allocation or foreign-currency
  original/functional amounts.
- Does not replace full `OpeningBalanceImport` / `OpeningBalanceLine`
  target-module persistence. Those scaffold models have not been mapped.
- Existing fiscal-period overlap and calendar change races outside the locked
  import/normal posting path require separate platform controls.
- These code and test changes are **not yet proof of a successful CI build,
  PostgreSQL regression run, production deployment or statutory correctness**.
  See `docs/ai/VALIDATION.md` for verifiable evidence and follow-ups.
