# ACCOUNTING-001C — transactional period and fiscal-year close gates
**2026-10-14 | IMPLEMENTED IN LEGACY RUNTIME — CI / POSTGRESQL VERIFICATION PENDING**

This is a **close-control foundation milestone**. It does **not** claim
full statutory year-end accounting, profit/loss transfer, reconciliation
of AR/AP/bank/inventory, tax returns, financial statement sign-off, or
implementation of the new 35-entity Accounting module.

## Existing public HTTP routes (now additionally protected)

`POST /api/v1/accounting/fiscal-years/{fiscalYearId}/periods/{periodId}/{soft-close|close|reopen}`

`POST /api/v1/accounting/fiscal-years/{fiscalYearId}/{soft-close|close|reopen}`

All six transitions now require **both** `accounting.write` and
`accounting.post` and the existing active-tenant authorization gate.
The existing endpoints still return 200 with a status response, 404 for
an unknown matching period, or 409 for a failed business gate.

## Period close invariants

- Lock the tenant-owned fiscal-year row `FOR UPDATE` followed by the
  accounting-period row `FOR UPDATE` inside a single EF database transaction.
  All status reads are made **after** locks are granted.
- Year must be **Open** to change a period's status; a closed/soft-closed
  year must first be reopened explicitly.
- To `SoftClose` or `Close` a period, all journal entries assigned to the
  period must be resolved (**no Draft journal**). None may have a
  `PostingDate` outside that period's start/end dates.
- `SoftClosed` and `Closed` periods prohibit new manual drafts, changes
  to draft header/lines and posting. An authorized period `Reopen` is
  required to resume editing or posting.
- Existing `JournalService.CreateAsync`, `UpdateAsync`, `AddLineAsync`,
  `UpdateLineAsync`, `DeleteLineAsync` now run in transactions and use
  journal or calendar row locks followed by open-period revalidation.
  This closes a race where creating/editing a draft could interleave
  with closing and violate the no-drafts gate.
- Previously implemented manual posting/reversal and sales/purchases/payments
  auto-posting use shared locks on the same fiscal-year/period rows. A close
  waits for already running financial postings; a subsequent posting sees
  the committed closed status and is rejected. The database's immutable
  posted-journal triggers and number-sequence transaction remain in force.
- Closing or reopening is auditable through the existing entity
  `UpdatedAtUtc` / `UpdatedByUserId` infrastructure; there is no new
  `PeriodCloseRun` evidence entity yet.

## Fiscal-year gates

- A year transition locks its **year** row `FOR UPDATE` and reads the
  current status before any checks.
- `SoftClose` requires one or more periods, **no Open period**, and
  **no remaining Draft journal** assigned to any year period.
- `Close` requires one or more periods, **every period Closed**, and
  **no Draft journal** assigned to any of those periods.
- `Reopen` changes the year status back to Open, but deliberately does
  not silently reopen any child period. Each child period requires its
  own authorized transition.
- Concurrent year close/reopen, period transitions, first-year opening
  balances and regular GL posting are serialized by PostgreSQL row locks.

## Added real HTTP + migrated PostgreSQL tests (not yet verified)

Six regressions added to the existing `FinancialHttpPostgresRegressionTests` fixture:

1. Reject close when a Draft exists, then allow close after posting;
   a new draft cannot be created once Closed.
2. Reject `SoftClose` with drafts, allow after posting, prohibit new drafts
   while SoftClosed, and verify authorized Reopen.
3. Refuse fiscal-year Close/SoftClose while periods remain Open; allow
   year Close after child Close; refuse child Reopen while year Closed;
   reopen year and then child.
4. Require all child periods Closed before hard year Close, even
   when the year and its periods are SoftClosed.
5. Reject cross-tenant period-close requests.
6. PostgreSQL lock contention: a second transaction holding a
   fiscal-year `FOR SHARE` row lock blocks period Close until released.

## Important unimplemented financial closing obligations

This milestone does **not** generate year-end P&L closing journal entries
or retained-earnings carry-forward. It does not ensure contiguous fiscal
period coverage of every date in the year, automated accruals/deferrals,
adjusting entries, fixed-asset depreciation, inventory valuation,
multi-currency FX remeasurement, VAT/payroll/AP/AR reconciliation,
bank matching, intercompany eliminations, consolidation, approval
workflow, signed close checklist, reopening reason/audit evidence or
financial-report snapshots. The fiscal-year endpoint is a **controlled
status change** only. These must become separately verified phase-2–5
features before the ERP or any jurisdiction-specific financial close is
called complete.

CI outcomes and regression run IDs belong in `docs/ai/VALIDATION.md`;
**no green CI, .NET compilation or PostgreSQL test execution is claimed
from creating these commits.**
