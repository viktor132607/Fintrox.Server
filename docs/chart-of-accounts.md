# Chart of Accounts

The Chart of Accounts is organization-scoped and stored in the `accounting` schema.

## Account model

Each account contains:

- code;
- name;
- account type;
- optional parent account;
- analytical flag;
- active/inactive state;
- manual-posting flag;
- common audit metadata.

Supported account types:

- Asset
- Liability
- Equity
- Revenue
- Expense
- OffBalance

## Invariants

- account codes are unique per organization;
- codes are normalized to uppercase;
- an account can only reference a parent from the same organization;
- child and parent must have the same account type;
- inactive accounts cannot be selected as parents;
- hierarchy cycles are rejected;
- an account cannot be its own parent;
- an account with active children cannot be deactivated;
- a child account cannot be activated while its parent is inactive;
- parent foreign keys use `RESTRICT`, so hierarchy cannot be removed through a cascading delete.

Accounts are deactivated instead of physically deleted.

## API

All requests require authentication and `X-Organization-Id`.

Read operations require `accounting.read`.

Write operations require `accounting.write`.

```text
GET    /api/v1/accounting/accounts
GET    /api/v1/accounting/accounts?includeInactive=true
GET    /api/v1/accounting/accounts/tree
GET    /api/v1/accounting/accounts/{accountId}
POST   /api/v1/accounting/accounts
PUT    /api/v1/accounting/accounts/{accountId}
DELETE /api/v1/accounting/accounts/{accountId}
POST   /api/v1/accounting/accounts/{accountId}/activate
```

## History

Account create/update/activate/deactivate operations are captured automatically by the audit foundation from phase 7.

A dedicated account-history query can therefore be added later without duplicating history tables.

## Deferred capabilities

Turnover and account balances depend on posted journal entries and are intentionally implemented after the double-entry Journal and reporting phases.

CSV/Excel/PDF import/export belongs to the shared export/import phase rather than duplicating file-processing logic inside the account aggregate.
