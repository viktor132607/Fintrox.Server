# ACCOUNTING-001D — transactional fiscal-year profit/loss closing

**Implemented in the active legacy ledger on 2026-10-14; build / CI / PostgreSQL regression execution UNVERIFIED.** This adds actual accounting entries, not merely fiscal-year status changes. It is still **not** complete statutory, tax or consolidated financial closing.

## Authorized API

`POST /api/v1/accounting/fiscal-years/{fiscalYearId}/close-profit-loss`

Body:

```json
{
  "retainedEarningsAccountId": "00000000-0000-0000-0000-000000000001"
}
```

Requires an active organization plus **both** `accounting.write` and
`accounting.post` permissions. Returns HTTP 200 with
`ProfitLossCloseResponse`: `fiscalYearId`, `journalEntryId`,
`netIncome` (positive profit, negative loss, 0 break-even) and
`alreadyClosed`. Unknown year scoped to a different tenant returns 404;
invalid accounting state/configuration returns 409.

## Core accounting rules

- Use the existing **authoritative** `accounting.journal_entries` /
  `accounting.journal_lines` tables and the existing fiscal-year number
  allocator, posted-ledger immutability protections, and journal audit fields.
  No direct cross-module ledger mutation and **no new migrations**.
- One transaction acquires PostgreSQL `FOR UPDATE` on the tenant fiscal-year
  row **before** reading year state or P&L balances. Regular manual/automated
  posting already acquires `FOR SHARE` on this row. This serializes the
  final result and year status against concurrent postings across instances.
- Requires fiscal year **Open** and an active tenant-owned
  **Equity** destination account explicitly selected by the caller.
- Requires a contiguous, gap-free, nonoverlapping fiscal calendar from
  `FiscalYear.StartDate` through `FiscalYear.EndDate`.
  Every period except the final one must be **Closed**. The final period must
  be **Open** and end on the fiscal year's last day.
- Requires **zero remaining Draft journals** assigned to any year period.
  Fiscal year and final period locks protect the close state during posting.
- Read posted **and Reversed** journals within the exact fiscal-year date
  bounds. Group debit/credit totals by active tenant-owned Revenue and Expense
  account. A posted-and-then-reversed journal contributes both original and
  reversing effects; draft journals contribute nothing.
- For each non-zero P&L account balance, issue the opposing journal line:
  debit balances are credited, credit balances are debited. The sum of
  `Credit - Debit` for the year is `netIncome`.
  A **positive result** credits retained earnings; a **negative result**
  debits retained earnings; for **zero net income** no equity line is needed.
  The journal is balanced to the existing four-decimal-place GL precision.
- A new **Draft** system journal is persisted first with its lines, then
  assigned a journal number and marked **Posted**. The last fiscal period
  and fiscal year both transition to **Closed** in the **same transaction**.
  A validation/SQL failure rolls back the journal, lines, sequence and states.
- Stable system external reference
  `profit-loss-close:v1:{fiscalYearId:N}` uses the existing unique
  tenant + System source external-reference index to prevent duplicate
  year-end journals. Identical replay on an already **Closed** year returns
  the same journal id/result with `alreadyClosed=true`.
  A changed retained earnings destination for a nonzero result is rejected.
  Reopened years with an existing closing journal require authorized
  correction rather than silently running the close again.
- The pre-existing **status-only** `POST .../fiscal-years/{id}/close` now
  refuses closure while any Revenue/Expense account has a non-zero annual
  balance, preventing users from bypassing the financial transfer.
  An empty fiscal year or a year whose P&L accounts are already individually
  zero can still use the old controlled status route.

## Scenarios added for real HTTP + migrated PostgreSQL fixture

Eight additional `FinancialHttpPostgresRegressionTests` cases
(written but **not yet executed / CI verified**):

1. Profit: close annual revenue/expense balances, credit retained earnings,
   atomically close last period and year, and verify same-ID retry.
2. Loss: debit retained earnings for a negative annual result.
3. Existing status-only close cannot bypass unsettled P&L.
4. Reject incomplete calendar, cross-tenant equity account and non-equity account.
5. Two concurrent close requests produce **one** closing journal.
6. Reject cross-tenant year and unresolved year Draft journal.
7. Zero-net-income year still closes distinct P&L accounts, no equity line.
8. Reject repeat close with a different retained-earnings destination.

## Explicit financial scope limits

This is a **single-base-currency, single-book GL P&L closing operation**,
not a legally certified annual close or a generalized accounting-policy engine.
Missing dependencies and later milestones:

- Configurable jurisdiction-specific closing accounts/intermediary profit
  and loss accounts, tax provision, deferred taxes and retained-earnings
  distribution or carried-forward losses.
- AP/AR/Banking/Inventory/Payroll/FixedAssets/Tax/Projects/Intercompany
  reconciliations and reconciliation evidence before authorizing final close.
- FX remeasurement, accrual/deferral schedules, depreciation, doubtful
  debts, valuation, posting dimensions and multiple books.
- Multi-level approval workflow, close checklist and signed close report;
  cross-year roll-forward/carry-forward balances and accounting-policy
  versioning.
- Status-only `Reopen` still exists for authorized corrections; if a
  closing journal already exists, rerunning this endpoint refuses to
  post a second closing journal. A full controlled reopen/reversal
  workflow is a **separate future task**.
- The 35 new Accounting module Domain entities remain unmapped scaffolds.
  These changes live in the proven legacy runtime until a parity-tested cutover.

All verification outcomes must be entered into
`docs/ai/VALIDATION.md` using actual CI job IDs and executed test results.
