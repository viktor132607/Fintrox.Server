# Legacy runtime model inventory — 2026-10-13

**32 existing model/persistence classes** retained in Fintrox.Server. This is the named **legacy migration inventory**, not a claim that all 32 are Domain entities or that the list covers every enum, helper, interface, EF configuration, or core service. These 32 are **not included** in the 936 newly organized module-owned Domain Entity classes.

Legacy means **current working implementation in the original horizontal** `src/Fintrox.Domain` and `src/Fintrox.Infrastructure` projects, with existing `FintroxDbContext` ownership. Do not delete them, reproduce their business logic in parallel, or replay/rename migrations until a tested feature-by-feature migration is ready.

## All 32 classes and future module ownership

| # | Existing class | Exact current source file | Intended target module |
|---:|---|---|---|
| 1 | `Account` | [`src/Fintrox.Domain/Accounting/Account.cs`](../../src/Fintrox.Domain/Accounting/Account.cs) | `Accounting` |
| 2 | `AccountingPeriod` | [`src/Fintrox.Domain/Accounting/AccountingPeriod.cs`](../../src/Fintrox.Domain/Accounting/AccountingPeriod.cs) | `Accounting` |
| 3 | `AutoPostingRule` | [`src/Fintrox.Domain/Accounting/AutoPostingRule.cs`](../../src/Fintrox.Domain/Accounting/AutoPostingRule.cs) | `Accounting` |
| 4 | `Currency` | [`src/Fintrox.Domain/Accounting/Currency.cs`](../../src/Fintrox.Domain/Accounting/Currency.cs) | `Currencies` |
| 5 | `ExchangeRate` | [`src/Fintrox.Domain/Accounting/ExchangeRate.cs`](../../src/Fintrox.Domain/Accounting/ExchangeRate.cs) | `Currencies` |
| 6 | `FiscalYear` | [`src/Fintrox.Domain/Accounting/FiscalYear.cs`](../../src/Fintrox.Domain/Accounting/FiscalYear.cs) | `Accounting` |
| 7 | `JournalEntry` | [`src/Fintrox.Domain/Accounting/JournalEntry.cs`](../../src/Fintrox.Domain/Accounting/JournalEntry.cs) | `Accounting` |
| 8 | `JournalLine` | [`src/Fintrox.Domain/Accounting/JournalLine.cs`](../../src/Fintrox.Domain/Accounting/JournalLine.cs) | `Accounting` |
| 9 | `IntegrationClient` | [`src/Fintrox.Domain/Integrations/IntegrationClient.cs`](../../src/Fintrox.Domain/Integrations/IntegrationClient.cs) | `Integrations` |
| 10 | `IntegrationEvent` | [`src/Fintrox.Domain/Integrations/IntegrationEvent.cs`](../../src/Fintrox.Domain/Integrations/IntegrationEvent.cs) | `Integrations` |
| 11 | `IntegrationFailure` | [`src/Fintrox.Domain/Integrations/IntegrationFailure.cs`](../../src/Fintrox.Domain/Integrations/IntegrationFailure.cs) | `Integrations` |
| 12 | `IntegrationInbox` | [`src/Fintrox.Domain/Integrations/IntegrationInbox.cs`](../../src/Fintrox.Domain/Integrations/IntegrationInbox.cs) | `Integrations` |
| 13 | `IntegrationRequestRecord` | [`src/Fintrox.Domain/Integrations/IntegrationRequestRecord.cs`](../../src/Fintrox.Domain/Integrations/IntegrationRequestRecord.cs) | `Integrations` |
| 14 | `WebhookDelivery` | [`src/Fintrox.Domain/Integrations/WebhookDelivery.cs`](../../src/Fintrox.Domain/Integrations/WebhookDelivery.cs) | `Integrations` |
| 15 | `WebhookSubscription` | [`src/Fintrox.Domain/Integrations/WebhookSubscription.cs`](../../src/Fintrox.Domain/Integrations/WebhookSubscription.cs) | `Integrations` |
| 16 | `Organization` | [`src/Fintrox.Domain/Organizations/Organization.cs`](../../src/Fintrox.Domain/Organizations/Organization.cs) | `Organizations` |
| 17 | `OrganizationMembership` | [`src/Fintrox.Domain/Organizations/OrganizationMembership.cs`](../../src/Fintrox.Domain/Organizations/OrganizationMembership.cs) | `Organizations` |
| 18 | `Counterparty` | [`src/Fintrox.Domain/Partners/Counterparty.cs`](../../src/Fintrox.Domain/Partners/Counterparty.cs) | `Counterparties` |
| 19 | `Payment` | [`src/Fintrox.Domain/Payments/Payment.cs`](../../src/Fintrox.Domain/Payments/Payment.cs) | `Payments` |
| 20 | `PaymentAllocation` | [`src/Fintrox.Domain/Payments/PaymentAllocation.cs`](../../src/Fintrox.Domain/Payments/PaymentAllocation.cs) | `Payments` |
| 21 | `PurchaseDocument` | [`src/Fintrox.Domain/Purchases/PurchaseDocument.cs`](../../src/Fintrox.Domain/Purchases/PurchaseDocument.cs) | `Purchases` |
| 22 | `PurchaseDocumentLine` | [`src/Fintrox.Domain/Purchases/PurchaseDocumentLine.cs`](../../src/Fintrox.Domain/Purchases/PurchaseDocumentLine.cs) | `Purchases` |
| 23 | `SalesInvoice` | [`src/Fintrox.Domain/Sales/SalesInvoice.cs`](../../src/Fintrox.Domain/Sales/SalesInvoice.cs) | `Sales` |
| 24 | `SalesInvoiceLine` | [`src/Fintrox.Domain/Sales/SalesInvoiceLine.cs`](../../src/Fintrox.Domain/Sales/SalesInvoiceLine.cs) | `Sales` |
| 25 | `VatCode` | [`src/Fintrox.Domain/Tax/VatCode.cs`](../../src/Fintrox.Domain/Tax/VatCode.cs) | `Tax` |
| 26 | `ApplicationUser` | [`src/Fintrox.Infrastructure/Identity/ApplicationUser.cs`](../../src/Fintrox.Infrastructure/Identity/ApplicationUser.cs) | `Identity` |
| 27 | `RefreshToken` | [`src/Fintrox.Infrastructure/Identity/RefreshToken.cs`](../../src/Fintrox.Infrastructure/Identity/RefreshToken.cs) | `Identity` |
| 28 | `AuditLogEntry` | [`src/Fintrox.Infrastructure/Audit/AuditLogEntry.cs`](../../src/Fintrox.Infrastructure/Audit/AuditLogEntry.cs) | `Audit` |
| 29 | `JournalNumberSequence` | [`src/Fintrox.Infrastructure/Persistence/Models/JournalNumberSequence.cs`](../../src/Fintrox.Infrastructure/Persistence/Models/JournalNumberSequence.cs) | `Accounting` |
| 30 | `PaymentNumberSequence` | [`src/Fintrox.Infrastructure/Persistence/Models/PaymentNumberSequence.cs`](../../src/Fintrox.Infrastructure/Persistence/Models/PaymentNumberSequence.cs) | `Payments` |
| 31 | `PurchaseDocumentNumberSequence` | [`src/Fintrox.Infrastructure/Persistence/Models/PurchaseDocumentNumberSequence.cs`](../../src/Fintrox.Infrastructure/Persistence/Models/PurchaseDocumentNumberSequence.cs) | `Purchases` |
| 32 | `SalesInvoiceNumberSequence` | [`src/Fintrox.Infrastructure/Persistence/Models/SalesInvoiceNumberSequence.cs`](../../src/Fintrox.Infrastructure/Persistence/Models/SalesInvoiceNumberSequence.cs) | `Sales` |

## Totals by legacy subsystem

- **Accounting: 8** — six target `Accounting`; `Currency` and `ExchangeRate` target `Currencies`.
- **Integrations: 7** — intended target: `Integrations`.
- **Organizations: 2** — intended target: `Organizations`.
- **Counterparties: 1** — intended target: `Counterparties`.
- **Payments: 2** — intended target: `Payments`.
- **Purchases: 2** — intended target: `Purchases`.
- **Sales: 2** — intended target: `Sales`.
- **Tax: 1** — intended target: `Tax`.
- **Identity: 2** — intended target: `Identity`.
- **Audit: 1** — intended target: `Audit`.
- **Persistence internals: 4** — sequence records map to Accounting, Payments, Purchases and Sales.

**Total: 32** (8 Accounting + 7 Integrations + 2 Organizations + 1 Counterparties + 2 Payments + 2 Purchases + 2 Sales + 1 Tax + 2 Identity + 1 Audit + 4 sequence records).

## Migration policy / status

| Aspect | Current | Required before legacy retirement |
|---|---|---|
| Ownership | Existing core `Fintrox.Domain`, `Fintrox.Infrastructure`, shared `FintroxDbContext` | Define single module owner and EF mapping migration strategy |
| Functional state | Existing legacy runtime routes, services and financial logic preserved | Characterization tests and functional parity for the migrating capability |
| Tenant/security | Existing authorization and tenant mechanisms remain authoritative | Cross-organization denial, permissions, and reference-boundary tests |
| Database | Existing EF mappings, migrations and sequence tables are authoritative | Backward-compatible migration, indexes, decimal precision, concurrency, upgrade/rollback checks |
| API contracts | Existing endpoints and contracts are the operational paths | Versioned module contracts with compatible consumers |
| Retirement | **Not started** | Remove old registration/implementation **only after** verified cutover |

### Classification notes

- Accounting's legacy `Currency` and `ExchangeRate` are targeted to the `Currencies` module, where the authoritative currency/rate aggregates must be reconciled before migration to avoid duplicate master data.
- `JournalNumberSequence`, `PaymentNumberSequence`, `PurchaseDocumentNumberSequence`, and `SalesInvoiceNumberSequence` live under Infrastructure/Persistence/Models rather than the Domain project. They are counted in the 32 migration records, **not** as new modular Domain Entity types.
- `ApplicationUser`, `RefreshToken` and `AuditLogEntry` are infrastructure-owned records with target module boundaries `Identity` and `Audit`.
- Legacy enums, abstract base classes, interfaces, DTOs, services, EF configuration classes, and other infrastructure machinery are **not part of this exact 32-class count**. They still require a separate dependency audit during each module migration.
- The 936 new module Domain models are presently constructor-validated definitions **without** corresponding EF mappings or module endpoints. Do not conflate model inventory completion with production migration.

Related: [module status](MODULE_STATUS.md), [entity expansion](ENTITY_EXPANSION_LOG.md), [migration architecture](../architecture.md), and [baseline audit](AUDIT.md).
