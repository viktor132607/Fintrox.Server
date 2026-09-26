# Accounting auto-posting rules

Phase 18 connects operational accounting documents to the double-entry journal through organization-scoped posting rules.

## Rule dimensions

Components:

- `AccountsReceivable`
- `AccountsPayable`
- `Revenue`
- `Expense`
- `OutputVat`
- `InputVat`
- `PaymentAsset`
- `CustomerAdvance`
- `SupplierAdvance`
- `FxGain`
- `FxLoss`

Match kinds:

- `Default`
- `ItemCode`
- `VatCode`
- `PaymentMethod`
- `ProductCategory`

`ProductCategory` is reserved for integration/business-event payloads. Current sales and purchase documents resolve revenue/expense by item code with a default fallback.

## Resolution

- receivable/payable/advance/FX components use default rules;
- revenue and expense use an item-code rule first, then default;
- output/input VAT use a VAT-code rule first, then default;
- payment assets use payment-method rules first, then default.

Rules reference active chart-of-account accounts and enforce compatible account types.

## Automatic entries

Sales invoice issue:

- Dr Accounts receivable
- Cr Revenue
- Cr Output VAT

Purchase receipt:

- Dr Expense, including non-recoverable VAT
- Dr Recoverable input VAT
- Cr Accounts payable

Incoming payment:

- Dr Payment asset
- Cr Accounts receivable for allocated carrying value
- Cr Customer advance for unallocated receipts
- Cr FX gain or Dr FX loss when required

Outgoing payment:

- Dr Accounts payable for allocated carrying value
- Dr Supplier advance for unallocated payments
- Cr Payment asset
- Cr FX gain or Dr FX loss when required

Foreign-currency document values are translated to base currency using the source exchange-rate snapshot. Payment settlement uses original document carrying values, so realized FX is recognized at payment confirmation.

## Transactionality and period locks

Automatic journal generation runs inside the same database transaction as:

- sales invoice issue;
- purchase document receipt;
- payment confirmation;
- source cancellation/reversal.

If mapping, fiscal-period validation, balancing, FX calculation, or journal posting fails, the operational state change is rolled back.

Posting is allowed only in open accounting periods and open fiscal years.

## Immutability and reversal

Generated entries use `JournalEntrySource.System` and deterministic external references:

- `sales-invoice:{id}`
- `purchase-document:{id}`
- `payment:{id}`

Only one system journal entry may exist per source reference.

Cancelling an issued/received/confirmed source creates and posts a reversing journal entry. Posted history is never edited.

Legacy documents created before phase 18 may not have a linked system journal entry; cancelling those documents remains supported and performs no accounting reversal.

## API

```text
GET    /api/v1/accounting/posting-rules
GET    /api/v1/accounting/posting-rules/{ruleId}
POST   /api/v1/accounting/posting-rules
PUT    /api/v1/accounting/posting-rules/{ruleId}
DELETE /api/v1/accounting/posting-rules/{ruleId}
POST   /api/v1/accounting/posting-rules/{ruleId}/activate
```

Permissions:

- read: `accounting.read`
- manage: `accounting.write`
