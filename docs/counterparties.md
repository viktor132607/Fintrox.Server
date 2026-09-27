# Counterparties

Phase 13 introduces organization-scoped counterparty master data shared by Sales, Purchases and Payments.

## Counterparty model

Each counterparty stores:

- internal code;
- display name;
- optional legal name;
- ISO-style two-letter country code;
- registration/company number;
- VAT number;
- customer flag;
- supplier flag;
- payment term days;
- contact person;
- email;
- phone;
- address lines;
- city;
- postal code;
- website;
- notes;
- active/inactive state;
- common audit metadata.

A counterparty must be a customer, supplier, or both.

## Uniqueness

The database enforces:

- unique counterparty code per organization;
- unique registration number per organization + country when present;
- unique VAT number per organization + country when present.

The application checks these rules before saving so normal conflicts return a controlled response instead of relying only on a database exception.

## Lifecycle

Counterparties are deactivated instead of physically deleted.

Historical accounting and commercial documents can therefore keep stable references later.

## Search and filters

The list endpoint supports:

- `includeInactive`;
- free-text `search`;
- `role=customer`;
- `role=supplier`;
- `role=both`.

Search covers:

- code;
- name;
- legal name;
- registration number;
- VAT number;
- email.

PostgreSQL `ILIKE` is used for case-insensitive search.

## Authorization

New permissions:

- `counterparties.read`
- `counterparties.write`

Role defaults:

- Owner: read/write
- Administrator: read/write
- Accountant: read/write
- Operator: read/write
- Viewer: read only

## API

All requests require authentication and `X-Organization-Id`.

```text
GET    /api/v1/counterparties
GET    /api/v1/counterparties/{counterpartyId}
POST   /api/v1/counterparties
PUT    /api/v1/counterparties/{counterpartyId}
DELETE /api/v1/counterparties/{counterpartyId}
POST   /api/v1/counterparties/{counterpartyId}/activate
```

## Audit

Counterparty create/update/activate/deactivate actions use the common audit pipeline and are recorded in the append-only audit log.

## Later integration

Sales invoices, purchase invoices, payments and journal analytical references will reuse the same counterparty IDs instead of creating separate customer/supplier master tables.
