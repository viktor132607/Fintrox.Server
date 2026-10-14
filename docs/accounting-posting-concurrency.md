# ACCOUNTING-001A — transactional GL posting vs fiscal close (2026-10-14)

## Scope and status

**Code implemented on `main`; CI and runtime regression verification pending.**
This is the first bounded ACCOUNTING-001 phase-2 foundation slice,
**not** completion of the Accounting, Tax, Currencies, or 36-module ERP.

The existing authoritative general ledger is still in the operational legacy
Accounting implementation. This change protects it without moving new module
Domain entity scaffolds or altering migrations/schema.

## Correctness invariant

No journal may transition to **Posted** (nor create a posted reversal) when
its fiscal year or accounting period was closed before the posting transaction
could acquire the calendar guard. Independent requests attempting to post or
reverse the **same** original journal must serialize and re-evaluate its most
recent committed status. A losing request must not consume a posting number.

## Implementation

1. `IJournalRepository.LockJournalForPostingAsync` acquires a
   PostgreSQL `SELECT ... FOR UPDATE` row lock on the tenant-scoped original
   journal **inside** the transaction, before reading tracked state.
   `JournalService.PostAsync`, `JournalService.ReverseAsync` and
   `AutoPostingEngine.ReverseEntryAsync` use this guard. Concurrent duplicate
   requests re-read the authoritative current status and reject or return
   without creating a second journal.
2. `IJournalRepository.LockPostingCalendarAsync` acquires PostgreSQL
   `FOR SHARE` locks on fiscal year **then** accounting period, ordered
   consistently and scoped to `organization_id`. Multiple postings can
   acquire shared locks simultaneously, while a closing `UPDATE` waits.
   If close won first, posting waits and sees the new closed status.
3. `JournalService.ResolveOpenPostingContextAsync` and
   `AutoPostingEngine.ResolveOpenPostingContextAsync` use the first lookup
   only to locate the calendar rows. They lock them, then re-read the
   period and fiscal-year statuses under `READ COMMITTED`; a changed period
   assignment/date or non-open status yields a conflict before any journal
   numbering/commit.
4. All locks require the caller's existing EF transaction (the
   `EfTransactionRunner` used by manual and sales/purchases/payment source
   services). Locks release on commit/rollback; no new transaction or
   persistent lock table is introduced.

The `AutoPostingEngine` adjustment applies the same fiscal close guard
to sales invoices, purchase documents and payments, and guards source
reversals against concurrent duplicate reversal. Source-specific posting
rules, tax recognition and integration idempotency remain separate backlog
items.

## PostgreSQL HTTP regression tests (added, not yet verified)

Added to `FinancialHttpPostgresRegressionTests`, using the existing real
ASP.NET Core host + migrated disposable PostgreSQL fixture:

- Simultaneous `POST .../journal/{id}/post`: exactly one HTTP 200 and
  one HTTP 409; one posted journal and one allocated sequence.
- Simultaneous `POST .../journal/{id}/reverse`: one HTTP 201 and
  one HTTP 409; one reversal and two sequence allocations in total.
- A concurrent transaction closes the accounting period while posting is
  in flight: posting waits, responds 409 and leaves the draft/no allocation.
- A concurrent transaction marks the fiscal year soft-closed while posting is
  in flight: posting waits, responds 409 and does not allocate a number.
  (The last test uses a direct SQL status transition to isolate the race
  condition; normal fiscal-year close workflow has additional guards.)

## Deferred work

- Full CI Release build, 4 added migrated PostgreSQL HTTP tests and existing
  regression suite must pass before marking ACCOUNTING-001A verified.
- Races involving concurrent **calendar reassignment**, fiscal-period
  creation/overlap, source-specific double posting or transaction deadlocks
  require separate coverage and/or full transaction discipline.
- Opening balances, recurring journals, accruals/deferrals, allocations,
  multi-book, dimensions, complete closing and localization remain pending.
- No new Accounting module EF mappings, cross-module finance contract,
  migration, UI, or deployed production verification is claimed.
