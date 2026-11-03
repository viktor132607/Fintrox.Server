# ACCOUNTING-001H1 — fixed monthly accrual/deferral recognition

Date: 2026-11-03. Status: CODED; **CI NOT YET VERIFIED**.

## Supported recognition kinds

| Kind | Debit | Credit | Meaning |
| --- | --- | --- | --- |
| PrepaidExpense | Expense | Prepaid asset | Release previously capitalized prepaid expenses |
| DeferredRevenue | Deferred liability | Revenue | Recognize previously deferred customer revenue |
| AccruedExpense | Expense | Accrued liability | Accrue an expense not yet paid/invoiced |
| AccruedRevenue | Accrued asset | Revenue | Accrue earned revenue not yet invoiced |

These are **GL recognition schedules**, not AP/AR invoices, cash movements,
tax documents or payment allocations. The existence and carrying value of an
original prepayment/deferred income balance is currently an accountant
responsibility; source-document linkage and balance sufficiency checks are
**NOT** implemented. Do not claim IFRS revenue/tax compliance from this slice.

## Scope

- Immutable, tenant-owned schedules, one of four supported kinds.
- Fixed posting day 1–28, initial posting date, 1–120 consecutive months.
- One positive 4-decimal amount in the tenant's GL base currency.
- Four-decimal allocation uses truncation to prevent allocating more than
  the total; the last period holds the exact residual so all periods sum to
  the total without loss.
- At schedule creation **and at each generation**, two distinct active
  tenant-owned analytical/eligible GL accounts must have correct account types.
- A single manual POST generates one immutable-evidence **System Draft** for
  a requested period; independent authorized GL posting is required.
- No automatic posting, retroactive regeneration after reversal, amendment,
  booking an initial prepayment, automatic accrual reversal or bank settlement.
- Source-controlled drafts cannot be changed via manual journal edit routes.
- Fiscal year and period are locked and must be open. Concurrent generation
  of one schedule/period is serialized with PostgreSQL row locking.
- Existing generation returns the same journal and its current status even
  when its fiscal period later closes; a posted/reversed journal is never
  silently regenerated.

## HTTP API

- GET /api/v1/accounting/recognition-schedules — accounting.read, 200 newest schedules.
- GET /api/v1/accounting/recognition-schedules/{id} — accounting.read,
  detailed 1–120 monthly occurrence projection, current GL status, 404 across tenants.
- POST /api/v1/accounting/recognition-schedules — accounting.write,
  immutable schedule creation (201).
- POST /api/v1/accounting/recognition-schedules/{id}/generate —
  accounting.write, body { "periodNumber": 1 }, idempotent generation (200).
- POST /api/v1/accounting/journal/{journalId}/post — accounting.post,
  separate posting after review.

Generation source reference:
recognition-schedule:v1:{scheduleId:N}:{periodNumber}

PostgreSQL migration 20261103000100_RecognitionSchedules adds
accounting.recognition_schedules and accounting.recognition_generations,
tenant-composite foreign keys, one-per-period and one-per-journal unique
constraints, required account/actor foreign keys, amount and kind checks,
and append-only UPDATE/DELETE triggers. Evidence and source journals are
created in the same transaction.

## Verification and remaining work

Five migrated PostgreSQL + HTTP regression tests cover allocation and
residual, immutable Draft → Posted, idempotency, four account-kind mappings,
invalid input/cross-tenant rejection, closed periods and parallel requests.
These tests are authored, **not yet executed** until GitHub CI confirms.

Next: validate warnings-as-errors build, all .NET tests, migration and
PostgreSQL, EF snapshot, Docker; repair failures before marking H1 complete.
ACCOUNTING-001H2: schedule revision/termination, accountant-approved
catch-up/corrections and controlled reversal tied to source documents.
Full statutory close and cross-module financial attestation remain pending.
