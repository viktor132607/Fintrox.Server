# Double-entry Journal

Phase 10 introduces the journal data model and draft-editing workflow.

Posting, entry numbering, posting-time balance validation, posted-entry immutability and reversal are intentionally implemented in phase 11.

## Journal entry

A journal entry stores:

- organization;
- nullable accounting number;
- posting date;
- document date;
- description;
- status;
- source;
- optional external reference;
- accounting/fiscal period;
- optional posted timestamp;
- common audit metadata.

Statuses are:

- `Draft`
- `Posted`
- `Reversed`

Sources are:

- `Manual`
- `Integration`
- `Import`
- `System`

The human journal API creates `Manual` draft entries only. Other sources are reserved for later integration/import/system workflows.

Draft entries intentionally have `Number = null`. The posting engine will assign the permanent accounting number atomically in phase 11.

## Journal line

Every line contains one account and two distinct amount columns:

- `Debit`
- `Credit`

This replaces the legacy design where one row held both a debit account and a credit account.

A line must:

- reference exactly one account;
- have a positive amount on exactly one side;
- never contain negative amounts;
- support at most four decimal places;
- reference an active account that allows manual posting when entered through the human API.

The database also enforces the single-sided amount rule with a check constraint.

## Tenant integrity

Tenant isolation is enforced both in application queries and with composite database foreign keys.

A journal entry can only reference an accounting period from the same organization.

A journal line can only reference:

- a journal entry from the same organization;
- an account from the same organization.

This prevents cross-company links even if faulty application code reaches the database.

## Fiscal period resolution

The API does not accept a caller-supplied period ID.

The period is resolved from the posting date using the fiscal calendar from phase 9.

Changing a draft posting date recalculates its fiscal period.

A draft can exist for a soft-closed or closed period. Whether it may actually be posted is a posting-engine concern and is enforced in phase 11.

## Draft API

All endpoints require authentication and `X-Organization-Id`.

Reads require `accounting.read`.
Draft modifications require `accounting.write`.

```text
GET    /api/v1/accounting/journal
GET    /api/v1/accounting/journal/{journalEntryId}
POST   /api/v1/accounting/journal
PUT    /api/v1/accounting/journal/{journalEntryId}

POST   /api/v1/accounting/journal/{journalEntryId}/lines
PUT    /api/v1/accounting/journal/{journalEntryId}/lines/{journalLineId}
DELETE /api/v1/accounting/journal/{journalEntryId}/lines/{journalLineId}
```

List filters:

```text
fromPostingDate
toPostingDate
status
source
```

The detail response includes debit/credit totals and an informational `IsBalanced` flag.

An unbalanced draft is allowed. Balance becomes mandatory only when phase 11 posts the entry.

## Audit

Journal entries and lines use the common auditable base classes.

Draft metadata and line changes therefore appear in the append-only audit log from phase 7.
