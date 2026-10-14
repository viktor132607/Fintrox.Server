# ACCOUNTING-001F4C3B1 — audited source-linked banking corrections

**2026-10-14 — CODE IMPLEMENTED IN main; .NET BUILD, EF MIGRATION AND POSTGRESQL TEST EXECUTION NOT VERIFIED.**

This is a **bounded first implementation** of ACCOUNTING-001F4C3B:
controlled reversals of ordinary **base-currency bank fee** and **unvalued
realized foreign-bank settlement** journals. It does **not** undo finalized
FX revaluations or approved bank close reviews. Those require an explicit,
versioned multi-source correction/approval cycle, not deleting old evidence.

## Endpoint

`POST /api/v1/banking/statements/{statementId}/lines/{lineId}/correct-posting`

Requires active organization, `accounting.write` **and** `accounting.post`.

Body example:

```json
{
  "sourceKind": "BankFee",
  "originalJournalEntryId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "correctionPostingDate": "2026-03-15",
  "reason": "This expense was categorized using an incorrect account"
}
```

`sourceKind` must be exactly `BankFee` or `ForeignSettlement`.
Reason is mandatory, trimmed, 10–400 characters so the complete
journal description remains within 500 characters. The posting date must not precede the original bank line date and **must remain in the original accounting period**, which must still be open within an open fiscal year. Historical or cross-period corrections require the future versioned workflow. Missing/other-tenant statement or
line returns 404; mismatched source journal, wrong category, closed
period or conflicting replay returns 409.

The response includes original and new reversal journal IDs,
`correctedByUserId`, source kind, correction date and `alreadyCorrected`.

## Immutable bank accounting correction semantics

New EF-discoverable migration
`20261014000600_BankPostingCorrections` creates
`banking.bank_posting_corrections`, an **append-only tenant-scoped audit
record** with statement line ID, allowed source kind, original journal ID,
compensating reversal journal ID, posting date, actor user ID, reason and
creation timestamp. Unique tenant/source line/kind, original journal and
reversal journal keys, composite tenant foreign keys, and a database
trigger rejecting UPDATE/DELETE prevent unaudited record replacement.

Inside one EF/PostgreSQL database transaction:

1. Read tenant-owned statement; acquire tenant bank-account **FOR UPDATE**
   then statement-line **FOR UPDATE** lock, same order as fee/FX booking.
2. Prove the original journal ID is the exact one already linked to that
   statement line via `bank_fee_postings` or `bank_fx_settlements`.
   **Do not accept a user-selected arbitrary journal**; check status
   Posted, source System, original reference
   `bank-fee:v1:{line:N}` or `bank-fx-settlement:v1:{line:N}`,
   original bank value date, and absence of prior reversal.
3. If `ForeignSettlement`, reject when an immutable FX revaluation or
   prepared bank closing exists with cutoff **at or after** the original
   bank value date. Correcting one component of a valued/approved period
   would invalidate both original GL carrying value and audit evidence.
   These cases must go to ACCOUNTING-001F4C3B2.
4. Check exact audit-link replay first; same actor/date/reason/original
   returns same original reversal ID, even if the period subsequently
   closes. A changed replay conflicts.
5. Resolve requested correction fiscal period and acquire GL fiscal-year then period locks, preventing concurrent close, before taking the original GL journal posting lock. The original journal must belong to **that same still-open period**, so no closed historical period is mutated.
6. Create a normal immutable **system reversal** via
   `JournalEntry.CreateReversalDraft`; reverse **every** original
   journal line in full (swap DR and CR, preserve line numbers/accounts),
   persist Draft+lines, allocate the year posting number, Post reversal,
   mark original `Reversed` with `ReversedByJournalEntryId`, insert
   immutable bank correction audit. Commit **all in one transaction**.
   A failure rolls everything back: no orphan compensating journal.
7. Generic journal reverse endpoint is also forbidden from reversing
   these bank compensation entries, just as it already rejects direct
   reversal of protected fee/FX source journals.

**Result is a correction requiring follow-up**, not a fake clean bank
reconciliation. Original bank statement line and source evidence stay
immutable. Bank statement detail now returns
`correctionReversalJournalEntryId` in addition to the original fee/FX
journal ID. The normal bank reconciliation only counts originally
**Posted** fee/FX source entries as processed, so the reversed source
falls back to an **unmatched bank line**, requiring a later properly
authorized accounting replacement or bank exception resolution.
Previously booked P&L and bank GL effects are cancelled by compensating
posting, but the original statement activity is not deleted.

## What this implementation refuses

- No correction of `bank_fx_revaluations` or fully/partially approved
  bank closing; does not edit old cutoff rate, original bank statement
  value, approved original record, or approval SHA.
- No correction of an `ExternalReference` not linked to the exact original
  bank line in the current organization. No backdating into a closed
  period or reversal before the bank transaction.
- No automatic re-booking to a new expense/clearing account: the corrective
  replacement **must be a separate explicitly authorized and audited
  workflow**; the original source is intentionally marked unresolved.
- No cancellation of original bank amount via this route. Wrong external
  bank statement data requires corrected imported bank evidence and
  versioned close history with an authorized bank-source correction.
- No general multi-module statutory financial close certification.

## Regression coverage authored but not run

**Eight** authenticated full HTTP/migrated PostgreSQL regression methods
in `tests/Fintrox.Domain.Tests/Accounting/FinancialHttpPostgresBankCorrectionTests.cs`:

1. Fee compensation creates reversed original, exact inverse Posted GL
   journal, immutable audit link, bank line remains unmatched and
   not falsely marked reconciled.
2. Same request replays original correction; changed reason conflicts;
   original fee source cannot be posted again.
3. Realized FX correction before cutoff valuation compensates all
   bank/clearing/FX gain or loss GL lines, preserving original bank line.
4. A settlement included in FX period-end revaluation is **not** reversed
   piecemeal.
5. Other tenant cannot access source; unrelated original journal/kind
   cannot be corrected.
6. Closed correction posting period rejects.
7. Generic journal endpoint cannot bypass protected source or re-reverse
   the compensation entry.
8. Two parallel identical submissions produce one reversal and one replay.

**Neither compilation nor execution has been demonstrated.** In particular,
the new migration and transactions need actual migrated PostgreSQL,
controller authorization and audit verification before production use.

## Next

`ACCOUNTING-001F4C3B2`: versioned **revaluation/approved bank close**
correction and controlled re-approval, including old and new rate
snapshots, native statement ledger bridge, reviewer sign-off and the
new historical source-to-GL links. These must be an atomic or explicitly
staged workflow with no replacement or deletion of previous audit rows.

After that: run and fix **all** accumulated ACCOUNTING-001 tests,
assess full fiscal close, tax and account reconciliation constraints.
`ACCOUNTING-001F` remains **IN PROGRESS**.
