# Fiscal years and accounting periods

Fintrox uses an organization-scoped fiscal calendar under the `accounting` schema.

## Fiscal years

Each fiscal year stores:

- name;
- start date;
- end date;
- status: `Open`, `SoftClosed` or `Closed`;
- standard audit metadata.

Rules:

- fiscal years cannot overlap within the same organization;
- fiscal year names are unique within an organization;
- only an open fiscal year can be structurally edited;
- an existing period cannot be pushed outside the fiscal year by editing the year dates;
- a fiscal year cannot be soft-closed while it has open periods;
- a fiscal year cannot be closed until every period is closed;
- a fiscal year with no periods cannot be closed;
- reopening a fiscal year does not automatically reopen its periods.

## Accounting periods

Each period stores:

- fiscal year;
- sequence number from 1 to 99;
- name;
- start date;
- end date;
- status: `Open`, `SoftClosed` or `Closed`;
- standard audit metadata.

Rules:

- periods must be fully contained within their fiscal year;
- period numbers are unique within a fiscal year;
- period date ranges cannot overlap within a fiscal year;
- only open periods can be structurally edited;
- period status cannot be changed unless the fiscal year itself is open;
- a closed period must be explicitly reopened before it can be soft-closed;
- tenant integrity is enforced by a composite fiscal-year/organization foreign key.

The repository also exposes a date lookup used by the future posting engine to resolve the period for a posting date.

## API

All endpoints require authentication and `X-Organization-Id`.

Read operations require `accounting.read`.
Write/status operations require `accounting.write`.

```text
GET    /api/v1/accounting/fiscal-years
GET    /api/v1/accounting/fiscal-years/{fiscalYearId}
POST   /api/v1/accounting/fiscal-years
PUT    /api/v1/accounting/fiscal-years/{fiscalYearId}
POST   /api/v1/accounting/fiscal-years/{fiscalYearId}/soft-close
POST   /api/v1/accounting/fiscal-years/{fiscalYearId}/close
POST   /api/v1/accounting/fiscal-years/{fiscalYearId}/reopen

GET    /api/v1/accounting/fiscal-years/{fiscalYearId}/periods
GET    /api/v1/accounting/fiscal-years/{fiscalYearId}/periods/{periodId}
POST   /api/v1/accounting/fiscal-years/{fiscalYearId}/periods
PUT    /api/v1/accounting/fiscal-years/{fiscalYearId}/periods/{periodId}
POST   /api/v1/accounting/fiscal-years/{fiscalYearId}/periods/{periodId}/soft-close
POST   /api/v1/accounting/fiscal-years/{fiscalYearId}/periods/{periodId}/close
POST   /api/v1/accounting/fiscal-years/{fiscalYearId}/periods/{periodId}/reopen
```

## Posting behavior

The actual posting engine is implemented in a later phase.

That engine will use these statuses to reject postings into closed periods and to apply the stricter soft-close policy.
