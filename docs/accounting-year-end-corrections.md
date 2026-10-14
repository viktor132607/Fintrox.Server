# ACCOUNTING-001E — controlled year-end corrections and GL reconciliation

**2026-10-14 | Code added in active legacy Accounting runtime | .NET build/CI/migrated PostgreSQL tests NOT YET VERIFIED.**

ACCOUNTING-001E addresses a critical financial invariant: a fiscal year
previously finalized with a posted P&L closing journal **must never**
be reopened by changing status alone. The financial transfer must be
reversed by an immutable, balanced and numbered **compensating journal**
within the same PostgreSQL transaction as reopening the year and final
period. Earlier closing entries and their journal numbers are preserved.

## APIs

### POST /api/v1/accounting/fiscal-years/{fiscalYearId}/reopen-profit-loss

Requires active tenant and both `accounting.write` and `accounting.post`.

```json
{
  "expectedClosingJournalEntryId": "00000000-0000-0000-0000-000000000001",
  "reason": "Correct year-end accruals following financial review"
}
```

The reason must be between **15 and 250 characters**. It is retained
in the compensating journal's immutable `Description` field as
`Year-end correction: <reason>`.

Response 200: `ProfitLossReopenResponse` (`fiscalYearId`,
`closingJournalEntryId`, `reversalJournalEntryId`,
`alreadyReopened`). 404 for another organization's/nonexistent year;
409 if the close is absent, wrong version, state or reason; 400 on
invalid request shape. Authorization middleware returns 401/403.

**Concurrency and idempotency:** the exact `expectedClosingJournalEntryId`
is mandatory, not merely a year. A request for an older closing version
**cannot** reopen the new closing round. Under a PostgreSQL year
`FOR UPDATE` lock, a successful first request:
1. Checks the year is `Closed`, the latest matching close is `Posted`,
   and the last period is `Closed` and owns that closing journal.
2. Locks the final period and original journal.
3. Creates a system `JournalEntry.CreateReversalDraft` posting exactly
   inverse debit/credit lines, with the reason in its description.
4. Persists the draft first, allocates a journal number, posts the
   compensating entry, marks the original `Reversed` with the
   `ReversedByJournalEntryId` link, and sets the final period and
   year to `Open`.
5. Commits the complete operation atomically. A database failure
   rolls back the sequence number, postings and fiscal states.

Existing database unique `reversal_of_journal_entry_id` plus the
year lock guards the one-compensating-journal invariant. An identical
retry while this closing version is still the latest returns the
existing reversal and `alreadyReopened=true`. A changed reason
returns 409. A delayed retry after reclose to a newer round returns
409 because the expected journal id is stale.

The old `POST .../fiscal-years/{id}/reopen` now explicitly **rejects**
status-only reopening when the year is Closed and has a P&L closing
journal. The generic `POST .../journal/{id}/reverse` now refuses
both P&L closing system journals **and their compensating year-end
reversal journals**, preventing a second standalone reversal from
corrupting fiscal results. Other normal journal reversals retain
their existing behavior.

### POST /api/v1/accounting/fiscal-years/{fiscalYearId}/close-profit-loss

Following an authorized correction in an Open final period, the
existing close command can run again. The immutable system references
are versioned:
- Original: `profit-loss-close:v1:<fiscalYearId:N>`
- Round 2: `profit-loss-close:v1:<fiscalYearId:N>:r2`
- Round 3: `profit-loss-close:v1:<fiscalYearId:N>:r3`

`IJournalRepository.ListYearEndClosingEntriesAsync` locates the
tenant's existing rounds. The latest must be reversed to post the next
round. The year must still be Open, the fiscal calendar continuous,
earlier periods Closed and final period Open. The new P&L balance
includes both the original closing lines and their compensating
reversal, so the close reflects all subsequent adjusting entries.
Posted journal IDs, numbers and audit timestamps from prior rounds
remain unchanged; **no destructive edits or deletes**.

### GET /api/v1/accounting/fiscal-years/{fiscalYearId}/close-reconciliation

Requires `accounting.read`. Returns
`FiscalCloseReconciliationResponse` computed under a PostgreSQL
fiscal-year lock from the authoritative tenant GL:
- Posted/reversed annual base-currency debit and credit totals.
- Unclosed income/expense debit and credit totals.
- Actual count of remaining Draft journal entries.
- Per-year contiguous fiscal calendar/no misdated journals.
- Previous periods Closed and last period Open readiness.
- Flags `ledgerBalanced`, `profitLossCleared`,
  `readyForProfitLossClose` and the latest closing journal ID.

**Warning:** `ledgerBalanced` is an **aggregate** GL check.
It does not certify every individual historical journal, subledger
or balance-sheet account. This is **not** a signed reconciliation,
reviewer approval or a legal closing checklist.

## Tests added (not executed / not verified)

Six HTTP + real migrated PostgreSQL regression methods use the
existing disposable database fixture, split into two partial-class
source files:

- Full close -> block unsafe reopen and direct reversal -> atomic
  compensating journal and exact reversed lines/statuses -> identical
  retry -> second, versioned close -> stale old retry rejected.
- Missing close, invalid correction reason and changed-reason retry.
- Readiness report before/after close including posted GL totals and
  cleared annual P&L.
- Outstanding draft count disables readiness.
- Foreign tenant correction rejected.
- Concurrent identical correction commands generate one reversal.

## Boundaries / next work

- No automatic balancing entries beyond the P&L closing reversal;
  accounting adjustments must use authorized existing journal APIs.
- Closing/reopening remains **single-company / single-ledger / base currency**.
- This does **not** validate external AP/AR/bank/stock/fixed-assets/
  payroll/tax reconciliations, inventory valuation, deferred taxes,
  FX remeasurement, intercompany settlements, consolidation or
  statutory reporting.
- A future-year financial opening/carry-forward, approval workflow,
  named approver/reason code, evidence attachments, cross-year
  rollback dependency checks and signed period-close checklist
  are not implemented.
- API-001 and ACCOUNTING-001A/B/C/D/E CI and runtime verification
  are still pending. No green CI or statutory compliance is claimed.
- Target-module Accounting EF persistence/API migration remains
  separate and pending; existing legacy GL remains authoritative.

Track execution and actual CI job IDs in `docs/ai/VALIDATION.md`.
