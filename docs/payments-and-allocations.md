# Payments and allocations

Phase 17 adds organization-scoped incoming and outgoing payments with settlement allocations.

## Directions and lifecycle

Directions:

- `Incoming` — customer receipt
- `Outgoing` — supplier payment

Statuses:

- `Draft`
- `Confirmed`
- `Cancelled`

Drafts are editable. Confirmed and cancelled payments are immutable.

## Payment methods

- `BankTransfer`
- `Cash`
- `Card`
- `DirectDebit`
- `Other`

## Numbering

Numbers are allocated only on confirmation.

Incoming:

```text
RCV-YYYY-NNNNNN
```

Outgoing:

```text
PAY-YYYY-NNNNNN
```

Sequences are isolated by organization, year and direction.

## Counterparties

Incoming payments require an active customer.

Outgoing payments require an active supplier.

On confirmation, the payment snapshots:

- counterparty name;
- registration number;
- VAT number.

## Currency and FX

Each payment stores:

- payment currency;
- organization base currency;
- payment-date exchange rate.

Convention:

```text
1 BaseCurrency = ExchangeRate × PaymentCurrency
```

Direct and inverse historical FX lookup are supported.

## Allocations

Incoming payments can allocate only to issued sales invoices.

Outgoing payments can allocate only to received purchase documents.

The target document must:

- belong to the same organization;
- belong to the same counterparty;
- be in its active settlement state.

One payment contains at most one allocation per target document.

A payment may remain partially or fully unallocated.

## Cross-currency settlement

Allocations store both:

- `DocumentAmount`
- `PaymentAmount`

The document currency exchange rate is resolved as of the payment date.

Conversion:

```text
PaymentAmount =
    DocumentAmount
    × PaymentExchangeRate
    / DocumentExchangeRate
```

This preserves payment-currency and document-currency settlement amounts independently.

Realized FX gain/loss accounting remains part of phase 18.

## Outstanding protection

Confirmed allocations count toward document settlement.

Cancelled payments do not count toward outstanding calculations, but their allocation rows remain for audit/history.

Before confirmation Fintrox:

1. locks every target invoice/document using PostgreSQL `FOR UPDATE`;
2. reloads and validates the target;
3. recalculates payment-date FX;
4. recalculates allocation payment amounts;
5. checks the target outstanding balance;
6. checks the total payment allocated amount;
7. confirms the payment atomically.

This prevents concurrent payments from over-allocating the same document.

## Settlement status

Settlement endpoints expose:

- document total;
- confirmed allocated amount;
- outstanding amount;
- `Unpaid`;
- `PartiallyPaid`;
- `Paid`.

## API

```text
GET    /api/v1/payments
GET    /api/v1/payments/{paymentId}
POST   /api/v1/payments
PUT    /api/v1/payments/{paymentId}

POST   /api/v1/payments/{paymentId}/allocations
PUT    /api/v1/payments/{paymentId}/allocations/{allocationId}
DELETE /api/v1/payments/{paymentId}/allocations/{allocationId}

POST   /api/v1/payments/{paymentId}/confirm
POST   /api/v1/payments/{paymentId}/cancel

GET /api/v1/payments/settlements/sales-invoices/{invoiceId}
GET /api/v1/payments/settlements/purchase-documents/{documentId}
```

List filters:

- `fromDate`
- `toDate`
- `status`
- `direction`
- `counterpartyId`

Authorization:

- `payments.read`
- `payments.write`

## Database

Tables:

```text
payments.payments
payments.allocations
payments.payment_number_sequences
```

Database constraints protect:

- positive payment amounts;
- allocated amount <= payment amount;
- lifecycle state;
- FX rates;
- allocation target XOR;
- positive allocation amounts;
- unique payment numbers;
- unique line numbers;
- one allocation per payment/target;
- organization-safe foreign keys.

PostgreSQL triggers make confirmed/cancelled payments and their allocations immutable except for the controlled `Confirmed → Cancelled` transition.

## Audit

Payments and allocations use the common append-only audit pipeline.

Automatic accounting entries remain phase 18.
