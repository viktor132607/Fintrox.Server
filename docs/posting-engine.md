# Posting engine and reversal

Phase 11 turns journal drafts into accounting records.

## Posting

Posting is an explicit action protected by `accounting.post`.

A draft can be posted only when all of the following are true:

- the journal entry is still `Draft`;
- its posting date resolves to the same fiscal period stored on the draft;
- the fiscal year is `Open`;
- the accounting period is `Open`;
- at least two lines exist;
- total debit is positive;
- total debit equals total credit exactly;
- every referenced account still exists;
- every referenced account is active;
- a manual journal uses only accounts that allow manual posting.

`SoftClosed` and `Closed` periods are both blocked from posting.

## Numbering

Permanent journal numbers are allocated only while posting.

Drafts remain unnumbered.

The database stores one sequence per organization and fiscal year in:

```text
accounting.journal_number_sequences
```

Sequence allocation uses an atomic PostgreSQL upsert inside the same transaction as posting.

The number format is:

```text
YYYYMMDD-NNNNNN
```

where the date prefix is the fiscal-year start date and the numeric part is the fiscal-year sequence.

A unique database index also protects journal numbers within the organization.

## Transaction boundary

Posting runs inside a database transaction.

The transaction contains:

1. reload and validate the draft;
2. reload the accounting period and fiscal year;
3. reload and validate all journal lines and accounts;
4. atomically allocate the next number;
5. mark the entry as `Posted`;
6. save audit data;
7. commit.

A failed validation or database operation rolls back the whole operation, including the number allocation.

## Immutability

Posted accounting records are not edited.

The API already prevents draft-edit endpoints from operating on posted entries.

Phase 11 additionally installs PostgreSQL triggers that prevent:

- editing accounting content on `Posted` entries;
- deleting `Posted` or `Reversed` entries;
- inserting, editing or deleting lines under a non-draft journal entry;
- modifying a `Reversed` entry.

The only allowed mutation of an existing posted entry is the controlled transition:

```text
Posted -> Reversed
```

with a link to the generated reversal entry.

## Reversal

Corrections are made with a new journal entry rather than rewriting history.

A reversal:

- requires the source entry to be `Posted`;
- can only be created once for that entry;
- requires an explicit reason;
- requires an open target fiscal year and period;
- copies the original lines;
- swaps debit and credit on every line;
- preserves the same accounts even if they were later deactivated;
- is created as `System` source;
- receives its own permanent posting number;
- is posted atomically;
- marks the original entry as `Reversed`;
- links both records through reversal IDs.

Reversal is also protected by `accounting.post`.

## API

```text
POST /api/v1/accounting/journal/{journalEntryId}/post
POST /api/v1/accounting/journal/{journalEntryId}/reverse
```

Reverse body:

```json
{
  "postingDate": "2026-09-25",
  "reason": "Correction of the original accounting treatment"
}
```

## Audit

Posting and reversal transitions use the common audit pipeline.

This keeps both accounting state and the actor/correlation history in the same transaction.
