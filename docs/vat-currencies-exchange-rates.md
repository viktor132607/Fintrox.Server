# VAT codes, currencies and exchange rates

Phase 14 adds organization-scoped tax and currency master data used by later Sales, Purchases, Payments and multi-currency accounting flows.

## VAT codes

VAT codes live in the `tax` schema and are organization-scoped.

Each code stores:

- code;
- name;
- VAT kind;
- rate percentage;
- validity range;
- applicability to sales and/or purchases;
- active/inactive state;
- common audit metadata.

Supported VAT kinds:

- `Standard`
- `Reduced`
- `ZeroRated`
- `Exempt`
- `OutOfScope`
- `ReverseCharge`

Rules:

- VAT rate is between 0 and 100;
- rates support up to four decimal places;
- zero-rated, exempt and out-of-scope codes must have a zero rate;
- standard and reduced codes require a positive rate;
- a code must apply to sales, purchases, or both;
- validity ranges cannot overlap for the same code in one organization;
- multiple historical versions of the same code are allowed when their date ranges do not overlap.

VAT versions are intentionally effective-dated so a future tax-rate change does not overwrite historical configuration.

API:

```text
GET    /api/v1/tax/vat-codes
GET    /api/v1/tax/vat-codes/{vatCodeId}
POST   /api/v1/tax/vat-codes
PUT    /api/v1/tax/vat-codes/{vatCodeId}
DELETE /api/v1/tax/vat-codes/{vatCodeId}
POST   /api/v1/tax/vat-codes/{vatCodeId}/activate
```

List filters:

```text
includeInactive
asOfDate
appliesTo=sales|purchases|both
```

## Currencies

Currencies live in the `core` schema.

Each currency stores:

- ISO-style three-letter code;
- name;
- optional symbol;
- decimal places;
- whether it is the organization's base currency;
- active/inactive state;
- audit metadata.

Rules:

- currency code is exactly three ASCII letters and stored uppercase;
- decimal places are between 0 and 4;
- currency code is unique per organization;
- at most one base currency may exist per organization;
- the database enforces the single-base rule with a partial unique index;
- the base currency must remain active;
- an inactive currency cannot be assigned as base;
- changing base currency is an explicit operation.

API:

```text
GET    /api/v1/currencies
GET    /api/v1/currencies/{currencyId}
POST   /api/v1/currencies
PUT    /api/v1/currencies/{currencyId}
DELETE /api/v1/currencies/{currencyId}
POST   /api/v1/currencies/{currencyId}/activate
POST   /api/v1/currencies/{currencyId}/set-base
```

No currency is auto-seeded. Each organization explicitly configures its own active currencies and base currency.

## Exchange rates

Exchange rates live in the `accounting` schema and are organization-scoped.

The rate convention is:

```text
1 BaseCurrency = Rate × QuoteCurrency
```

Each exchange rate stores:

- base currency;
- quote currency;
- effective date;
- rate;
- optional source;
- audit metadata.

Rules:

- base and quote currencies must be different;
- both currencies must belong to the same organization;
- new rates require both currencies to be active;
- rate must be positive;
- rate supports up to ten decimal places;
- only one rate may exist for one organization/currency pair/effective date;
- historical rates remain readable even after a currency is deactivated.

API:

```text
GET  /api/v1/exchange-rates
GET  /api/v1/exchange-rates/{exchangeRateId}
GET  /api/v1/exchange-rates/latest
POST /api/v1/exchange-rates
PUT  /api/v1/exchange-rates/{exchangeRateId}
```

List filters:

```text
baseCurrencyId
quoteCurrencyId
fromDate
toDate
```

Latest lookup:

```text
GET /api/v1/exchange-rates/latest
    ?baseCurrencyId={id}
    &quoteCurrencyId={id}
    &asOfDate=2026-09-25
```

If `asOfDate` is omitted, the current UTC date is used.

The latest lookup returns the newest rate whose effective date is less than or equal to the requested date.

## Authorization

New permissions:

```text
tax.read
tax.write
currencies.read
currencies.write
```

Default role behavior:

- Owner: read/write
- Administrator: read/write
- Accountant: read/write
- Operator: read only for tax/currency configuration
- Viewer: read only

## Audit

VAT codes, currencies, base-currency changes and exchange-rate changes use the common append-only audit pipeline.

## Future use

Later phases will reuse these IDs and historical values for:

- sales invoices;
- purchase invoices;
- payments;
- VAT reporting;
- document currency conversion;
- foreign-currency journal details;
- automatic accounting rules.
