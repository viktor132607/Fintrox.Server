# Purchase invoices and expenses

Phase 16 adds organization-scoped purchase documents for supplier invoices and general expenses.

## Document types

Supported types:

- `Invoice`
- `Expense`

Both use the same line/tax/currency model.

## Lifecycle

Statuses:

- `Draft`
- `Received`
- `Cancelled`

Drafts are editable and unnumbered internally.

Receiving performs final validation, snapshots supplier/currency data and atomically allocates the internal number.

Received and cancelled documents are immutable.

Automatic journal posting remains part of phase 18.

## Numbering

Internal numbers use:

```text
PUR-YYYY-NNNNNN
```

The sequence is scoped by organization and calendar year and is allocated inside the receive transaction.

`SupplierDocumentNumber` remains separate from the internal number.

For `Invoice` documents the supplier number is mandatory before receive.

When supplied, the supplier number is unique per organization + supplier.

## Supplier snapshot

At receive time Fintrox snapshots:

- supplier name;
- legal name;
- registration number;
- VAT number;
- country;
- address;
- city;
- postal code.

The selected counterparty must be active and marked as a supplier.

## Currency

At receive time Fintrox snapshots:

- transaction currency;
- organization base currency;
- effective exchange rate as of the document date.

Rate convention:

```text
1 BaseCurrency = ExchangeRate × DocumentCurrency
```

The service supports direct or inverse historical rate lookup.

## Lines

Each line contains:

- item code;
- description;
- quantity;
- unit;
- unit price;
- discount;
- VAT code/version reference;
- VAT code/rate snapshot;
- recoverable VAT percentage;
- net amount;
- VAT amount;
- recoverable VAT amount;
- non-recoverable VAT amount;
- gross amount.

The VAT code must be active, effective on the document date and applicable to purchases.

## Recoverable VAT

`RecoverableVatPercent` is between 0 and 100.

This allows purchase VAT to be split into:

- recoverable input VAT;
- non-recoverable VAT.

Totals preserve both values for the later accounting posting rules.

## API

```text
GET    /api/v1/purchases/documents
GET    /api/v1/purchases/documents/{documentId}
POST   /api/v1/purchases/documents
PUT    /api/v1/purchases/documents/{documentId}

POST   /api/v1/purchases/documents/{documentId}/lines
PUT    /api/v1/purchases/documents/{documentId}/lines/{lineId}
DELETE /api/v1/purchases/documents/{documentId}/lines/{lineId}

POST   /api/v1/purchases/documents/{documentId}/receive
POST   /api/v1/purchases/documents/{documentId}/cancel
```

List filters:

- `fromDate`
- `toDate`
- `status`
- `type`
- `counterpartyId`

Authorization:

- `purchases.read`
- `purchases.write`

## Database

Tables:

```text
purchases.documents
purchases.document_lines
purchases.purchase_document_number_sequences
```

The database enforces tenant-safe foreign keys, totals, lifecycle rules, unique internal numbers and supplier-document uniqueness.

Migration-level PostgreSQL triggers protect received/cancelled headers and their lines from later edits.

## Audit

Purchase documents and lines use the common append-only audit pipeline.
