# Trial Balance and General Ledger

Phase 12 adds read-only accounting reports over posted journal data.

The reports never read draft entries.

Entries with status `Reversed` remain part of accounting history because their economic effect is cancelled by a separate posted reversal entry on the reversal date.

## Trial Balance

Endpoint:

```text
GET /api/v1/reports/trial-balance
```

Required query parameters:

```text
fromDate
toDate
```

Optional:

```text
includeZeroBalances=false
```

Each account row contains:

- opening debit balance;
- opening credit balance;
- period debit turnover;
- period credit turnover;
- closing debit balance;
- closing credit balance.

The response also contains total opening, turnover and closing debit/credit values.

Opening balance is calculated from all posted accounting movements before `fromDate`.

Period turnover includes movements from `fromDate` through `toDate`, inclusive.

Closing balance is:

```text
opening net + period debit - period credit
```

Inactive accounts are preserved in the report when they contain historical movement.

When `includeZeroBalances=true`, all chart-of-accounts rows are returned.

## General Ledger

Endpoint:

```text
GET /api/v1/reports/general-ledger/{accountId}
```

Required query parameters:

```text
fromDate
toDate
```

The report contains:

- selected account metadata;
- opening debit/credit balance;
- period debit/credit turnover;
- closing debit/credit balance;
- chronological accounting movements.

Each movement contains:

- journal entry ID;
- permanent journal number;
- posting date;
- document date;
- journal description;
- journal line number;
- line description;
- source;
- external reference;
- debit;
- credit;
- running debit/credit balance.

The running balance starts from the opening net balance and is recalculated after every movement.

## Accounting history semantics

Reports include journal entries in either:

- `Posted`
- `Reversed`

They exclude:

- `Draft`

A reversed original entry must remain visible historically. Its linked reversal entry carries the opposite debit/credit effect from its own posting date.

This makes reports before and after the reversal date both correct.

## Database execution

Trial Balance aggregation is executed in PostgreSQL and grouped by account.

General Ledger uses:

- one aggregate query for opening net balance;
- one date-scoped query for movements.

The reporting service does not load the entire journal into application memory.

## Authorization

Both reports require:

```text
reports.read
```

Organization isolation continues to use `X-Organization-Id` and membership authorization.

## Schema

Phase 12 introduces no new database tables.

It uses the journal/account indexes already created by the Chart of Accounts and Double-entry Journal phases, so no additional EF migration is required.
