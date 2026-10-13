# Fintrox — enterprise financial system of record

**Decision: 2026-10-13. Status: architectural contract (not a claim that every workflow is implemented).**

## Non-negotiable outcome

**Fintrox owns the complete enterprise financial lifecycle.** Every company or organization that uses Fintrox must be able to originate, import, approve, record, reconcile, analyze and audit **all of its financial transactions** without requiring a second accounting product. Other apps may operate shops, services, warehouses or other nonfinancial experiences, but Fintrox is the authoritative financial back office for them.

"Centralized" means **one Fintrox platform and one authoritative financial record per organization**, **not** one giant Accounting module or a shared database where all domains write journal lines. Domain modules own their operational facts; only **Accounting** owns the authoritative general ledger/posting engine. All systems integrate through explicit, versioned contracts, never direct writes into another module's tables.

The old working financial runtime in `src/Fintrox.Domain`, `src/Fintrox.Application`, and `src/Fintrox.Infrastructure` remains authoritative until a tested migration replaces it. New Domain entities alone do **not** implement the flow.

## Coverage: every financial stream must terminate in Fintrox

| Enterprise stream | Source/owner within Fintrox | Ledger/cash/financial outcome |
|---|---|---|
| Organizational financial setup | Organizations, Identity, Capabilities | Legal entity, fiscal policy, access, approval limits, functional currency |
| Customers, suppliers, contracts | Counterparties, ContractManagement, CRM | Identifiers, payment terms, obligations, credit limits |
| Quotations, orders, invoicing, credits | Sales, OrderManagement, Commerce | Accounts receivable, tax, revenue, advances, collections and refunds |
| Requisitions, purchase orders, bills, expenses | Purchases, SupplyChain | Accounts payable, input tax, accruals, landed cost, supplier settlement |
| Cash, transfers, banking fees, bank feeds | Payments, Banking | Cash/bank subledgers, payment allocations, matching, reconciliation |
| POS, subscriptions, usage, payment gateways | Commerce, SubscriptionBilling, Integrations | Fintrox-originated invoices/receipts, deferred revenue, settlement, refunds |
| Product costs, receipts, stock, production | Inventory, WarehouseManagement, Manufacturing, QualityManagement | Inventory valuation, WIP, COGS, write-offs, manufacturing variances |
| Capital assets and asset financing | FixedAssets, Purchases | Acquisition, capitalization, depreciation, impairment, disposal |
| Personnel costs and liabilities | HumanResources, Payroll | Salaries, withheld tax, employer charges, payments, accruals |
| Projects and resource costs | Projects, ResourceManagement | Cost allocations, project revenue/cost, WIP and profitability |
| Recognition, deferred revenue, contracts | RevenueManagement | Recognition schedules, unearned/accrued revenue, reversals |
| Currencies, rates and FX differences | Currencies | Currency translation, revaluation, rounding, realized/unrealized FX |
| Tax computation and declarations | Tax | VAT registers/returns, withholding and fiscal reports; jurisdiction-localized rules |
| Forecast, commitments and budgets | Budgeting | Budget vs actual, scenario planning, liquidity forecasts and commitments |
| Group consolidation and intercompany | Consolidation | Intercompany matches, eliminations, ownership, translation and group statements |
| Approvals, documents and audit | Workflows, Documents, Audit | Evidence, version history, segregation of duties, full traceability |
| Statements and management information | Reporting, Accounting | Trial balance, GL, P&L, balance sheet, cash flows, aging, cash position, profitability |
| External applications and user experiences | Integrations, Experience, Capabilities | Versioned, secure submission and consumption; no shadow ledger outside Fintrox |

**Financial completeness means all of:** GL and subledgers, AP/AR, receivables/payables aging, taxes, fiscal periods, fixed assets, inventory costing, payroll, expense reimbursements, bank/cash, payment provider fees, multiple currencies, revenue recognition, budgets, forecasts, intercompany, consolidation, audit and reporting. Optional modules may be deactivated for a business that does not use them, but financial facts they generate must still be integrated into this financial record.

## Canonical flow

```text
External app / Fintrox operational module / manual entry / bank feed
   -> trusted, tenant-scoped API or integration contract
   -> source document or accepted immutable source event
   -> validation (tenant, permissions, amounts, currencies, periods, taxes)
   -> approval / segregation of duties / policy gates
   -> deterministic, idempotent financial posting intent
   -> one Accounting posting engine -> balanced journal in the GL
   -> subledger and cash/bank reconciliation
   -> reporting / tax filing / group consolidation / audit
```

A successful external API call **does not** imply a payment cleared, an invoice was legally issued, or a journal was posted. Those are distinct state transitions, confirmed by the authoritative Fintrox module.

### Required source reference and idempotency

Every externally or operationally generated financial event must carry at least:
`OrganizationId`, `SourceSystem`, `SourceEntityType`, `SourceEntityId`, `SourceVersion` or equivalent revision, `EventId`, `CorrelationId`, event timestamp, document/booking dates, normalized currency code, amounts/lines and tax codes where applicable. Authenticate the actor, authorize the organization and permitted scope, validate ownership and cross references.

- Deduplicate delivery with a unique tenant + producer + event identity. Deduplicate financial *effect* against the tenant + business document identity + revision/posting action. At-least-once delivery must not result in double posting.
- Use transactional inbox/outbox (or equivalent durable workflow), atomic document/posting state transitions, retries, poisoned-event isolation and reconciliation of external references.
- Reject changes to posted ledger transactions. Make corrections through authorized reversals and adjusting entries with links to source documents.
- Money is `decimal` with explicit precision, currency/rounding policy and auditable FX rate snapshots. Never `float`/`double` for amounts.
- Keep **immutable audit history** (actor, action, previous/new state, timestamp, reason and correlation/source references). Prefer append-only events for posting and payment finality.
- Distinguish `requested`, `authorized`, `captured`, `settled`, `reconciled`, `posted` and `reversed` where the business process requires them.
- Reject closed-period posting unless an explicitly authorized adjustment/reopening policy exists. Generate the required period-end checks/reconciliations.
- User roles, external integration scopes and approval limits are always checked **server-side**; deactivated tenants cannot perform finance operations.
- No hidden cross-module EF navigations, direct cross-schema SQL writes, or bypass routes from external projects into `JournalLine`. Accounts, invoices, payments and ledger facts have a **single authoritative owner**.
- One tenant's information must not be visible or posted to another. Intercompany movements are explicit paired documents with their own approvals and eliminations—not accidental cross-tenant references.

## Example: online sale paid by gateway

1. Orisia or another storefront creates an operational order and submits a versioned sale to Fintrox Commerce/Sales with an idempotency key.
2. Fintrox validates price, inventory/reference context, fiscal/tax jurisdiction and creates the authoritative invoice or receipt.
3. Fintrox Accounting posts **receivable/cash versus revenue and tax** based on the applicable account mapping and actual legal document state.
4. Gateway `authorized`/`captured`/`settled` notifications are processed separately in Payments; Banking matches the settlement including provider fees.
5. Refund or return creates a credit note and correctly authorized reversal/adjustment; double delivery cannot duplicate the accounting effect.
6. Tax, bank reconciliation, customer balance, inventory cost and P&L are traced to the original order and documents in Fintrox.

**Do not assume every step can post automatically.** Jurisdiction-specific recognition, VAT/fiscal receipt rules, accounting policies, payment semantics and approvals require explicit implementations and regression tests.

## Milestone order and acceptance gates

1. **Identity/Organizations (phase 1):** user/service-client identity, tenant-active gate, role/scope permissions and reactivation; protect finance data by company. ORG-002 implements a first live security gate, not complete identity migration.
2. **Accounting (phase 2):** characterize existing GL, posting and reversal flows with real PostgreSQL tests; define posting contracts and own ledger boundary before migrating existing legacy classes.
3. **Counterparties/Currencies/Tax (phase 2):** master data, money/rates, tax point, jurisdiction rules, fiscal years and period control.
4. **Sales/Purchases/Payments/Banking (phase 3):** source-to-ledger invoice/bill/credit/settlement flows, AP/AR and reconciliation; integration idempotency.
5. **Inventory/Manufacturing/Payroll/FixedAssets/Projects/Budgeting (phases 4 & 7):** valuation, accruals, cost and liability posting, commitments and planning.
6. **RevenueManagement/SubscriptionBilling/Commerce (phases 7 & 9):** recognition, usage billing, POS, subscriptions, adjustments.
7. **Consolidation/Reporting/Audit/Documents/Workflows (phases 5 & 9):** traceable closing, statutory/managerial views, consolidation and approvals.
8. **Integrations/Capabilities/Experience (phases 6 & 8):** activate/connect Fintrox to the user's other applications, with Simple/Accountant/Expert interfaces to the *same* financial data and server-enforced permissions.
9. **Production hardening (phase 10):** regulatory/localization verification, privacy, audit, performance, concurrency, backup/recovery and production deployment gates.

**Acceptance for any completed financial feature:** an end-to-end trace from source document to subledger and balanced journal, verified posting/reversal/idempotency behavior, tenant and authorization denial tests, precision/FX/tax treatment, real PostgreSQL/migration tests, readable API contracts, updated module README and `docs/ai/MODULE_STATUS.md`. Do **not** mark a module COMPLETE because its Entity files exist.

## External product integration boundary

Other applications (e.g. e-commerce, service portals, business dashboards) may keep product UX and nonfinancial domain logic. They **must not** become a separate accounting ledger or competing issuer of financial documents that Fintrox is meant to own. Implement a Fintrox API/SDK adapter for each product and support safe organization-scoped activation through Integrations/Capabilities. For legacy commerce products, migrate gradually with reconciliation/parity until Fintrox is demonstrably authoritative; do not delete existing runtime data or disable checkout by fiat.

### Source of truth / migration dependencies

- Existing runtime: [legacy model inventory](ai/LEGACY_MODEL_INVENTORY.md); new Domain catalog: `architecture/domain-entities.json`.
- Work queue: [TASK_QUEUE](ai/TASK_QUEUE.md); phased sequence: [MASTER_ROADMAP](ai/MASTER_ROADMAP.md).
- This specification overrides an interpretation of "centralized" as a giant mutable `DbContext` shared among all modules. Financial governance is centralized in Fintrox, while ownership and code boundaries remain modular.
