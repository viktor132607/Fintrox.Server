# Sales invoices

Phase 15 introduces organization-scoped sales invoices with immutable issued documents, customer/VAT/currency snapshots and deterministic totals.

## Lifecycle

Sales invoices use:

- `Draft`
- `Issued`
- `Cancelled`

Drafts are editable and unnumbered.

Issuing performs the final validation and atomically allocates the permanent invoice number.

Issued invoices are immutable. Cancellation preserves the original invoice and records a cancellation timestamp and reason.

Automatic journal posting is intentionally not part of this phase. It is implemented later by the accounting auto-posting rules phase.

## Numbering

Permanent numbers are allocated only when an invoice is issued.

Sequence storage:

```text
sales.sales_invoice_number_sequences
```

The sequence is scoped by organization and calendar year.

Format:

```text
INV-YYYY-NNNNNN
```

Allocation runs inside the same PostgreSQL transaction as invoice issuing, so failed issues do not consume a committed invoice number.

## Customer snapshot

A sales invoice references the counterparty by ID, but issued documents also preserve a snapshot of:

- customer name;
- legal name;
- registration/company number;
- VAT number;
- country;
- address;
- city;
- postal code.

The snapshot is refreshed from the current counterparty master data immediately before issuing.

Later edits to the counterparty therefore do not rewrite an already issued invoice.

The selected counterparty must be active and marked as a customer.

## Currency snapshot and exchange rate

Each invoice has a transaction currency.

At issue time Fintrox also snapshots:

- transaction currency code;
- current organization base currency ID;
- current base currency code;
- exchange rate.

Exchange-rate convention:

```text
1 BaseCurrency = ExchangeRate × InvoiceCurrency
```

If the invoice currency is the base currency, the rate is `1`.

For foreign currency invoices the service first searches for a direct base-to-invoice rate as of the invoice date. If only the inverse pair exists, the reciprocal is used.

An invoice cannot be issued without an active organization base currency and an available rate for a foreign currency.

## Lines and calculation

Each line stores:

- line number;
- optional item code;
- description;
- quantity;
- optional unit of measure;
- unit price;
- discount percentage;
- VAT code reference;
- VAT code snapshot;
- VAT rate snapshot;
- net amount;
- VAT amount;
- gross amount.

Calculation:

```text
undiscounted = quantity × unit price
discounted   = undiscounted × (1 - discount% / 100)
net          = rounded discounted amount
vat          = rounded net × VAT rate
gross        = net + vat
```

Rounding uses the configured decimal places of the invoice currency and `AwayFromZero`.

Supported input precision:

- quantity: 6 decimal places;
- unit price: 4 decimal places;
- discount: 4 decimal places;
- VAT rate: 4 decimal places.

Invoice totals are maintained from the line snapshots:

- net total;
- VAT total;
- gross total.

An invoice must contain at least one line and have a positive gross total before it can be issued.

## VAT validation

Each invoice line references an effective-dated VAT-code version.

A line can only use a VAT code that is:

- in the same organization;
- active;
- valid on the invoice date;
- applicable to sales.

Before issuing, every VAT reference is revalidated.

If a VAT master-data version was modified after the line was created, issuing is blocked until the line is explicitly updated. This prevents silent tax changes during finalization.

## Due date

The caller may provide an explicit due date.

If omitted, the due date is derived from the counterparty payment terms:

```text
invoice date + payment term days
```

Due date cannot be before invoice date.

## Draft editing rules

Draft invoices support:

- header update;
- line add;
- line update;
- line delete.

Changing invoice currency after lines exist is blocked because line rounding depends on currency decimal places.

Changing invoice date is allowed only if all existing VAT-code versions remain valid on the new date.

## API

All endpoints require authentication and `X-Organization-Id`.

Reads require `sales.read`.

Writes, issuing and cancellation require `sales.write`.

```text
GET    /api/v1/sales/invoices
GET    /api/v1/sales/invoices/{invoiceId}
POST   /api/v1/sales/invoices
PUT    /api/v1/sales/invoices/{invoiceId}

POST   /api/v1/sales/invoices/{invoiceId}/lines
PUT    /api/v1/sales/invoices/{invoiceId}/lines/{lineId}
DELETE /api/v1/sales/invoices/{invoiceId}/lines/{lineId}

POST   /api/v1/sales/invoices/{invoiceId}/issue
POST   /api/v1/sales/invoices/{invoiceId}/cancel
```

List filters:

```text
fromDate
toDate
status
counterpartyId
```

## Database integrity

Tables:

```text
sales.invoices
sales.invoice_lines
sales.sales_invoice_number_sequences
```

Composite tenant foreign keys prevent cross-organization links to:

- counterparties;
- currencies;
- base currencies;
- VAT codes;
- invoice headers.

Database constraints also protect:

- due date;
- totals;
- positive exchange rate;
- invoice lifecycle consistency;
- line quantity;
- unit price;
- discount range;
- VAT range;
- line amount arithmetic.

The migration installs PostgreSQL triggers that block editing/deleting issued or cancelled invoices and block insert/update/delete of lines under non-draft invoices.

## Audit

Invoices and invoice lines use the common auditable entity base classes.

Create/update/issue/cancel and line changes therefore flow through the append-only audit log.
