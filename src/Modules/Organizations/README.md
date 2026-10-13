# Organizations

Status: PARTIAL — Domain entity foundation implemented; persistence, application use cases and API pending.

Owns: Companies, Memberships, Settings, Branches.
Reserved PostgreSQL schema: `organizations` (not created).
Existing implementation/reference: Organizations; core schema.

## Boundaries

- Domain: pure model, value objects, invariants and internal domain events.
- Application: feature-oriented use cases; ports in Abstractions. Commands/queries belong under their feature when implemented.
- Contracts: versioned DTOs and integration events; no entities, EF, ASP.NET or implementation dependencies.
- Infrastructure: module-owned context, configurations, migrations, messaging and external adapters.
- Presentation: thin HTTP endpoints and transport mapping; no persistence access.
- Composition: the host explicitly wires Infrastructure and Presentation when implementation is authorized.

Foreign data is referenced by identifiers/snapshots and accessed through approved Contracts.
No cross-module DbContext, repositories, entity navigations, SQL joins, foreign keys or shared transactions in the target architecture.
See [architecture](../../../docs/architecture.md) for the current legacy exceptions and migration gates.

## Implemented entity foundation

`Branch`, `Department`, `BusinessUnit`, `CompanyAccountingPolicy`, `TaxRegistration`.

Models live in this module Domain/Entities directory; foreign objects are referenced by IDs. These are domain types, not registered database tables or endpoints. Existing legacy workflows remain in core projects. See [module progress](../../../docs/ai/MODULE_STATUS.md) for inventory and remaining work.

## Domain expansion — 2026-10-13

Added 18 validated Domain entities (23 total): `OrganizationLegalEntity`, `OrganizationAddress`, `OrganizationContact`, `OrganizationHierarchy`, `OrganizationMemberProfile`, `OrganizationMemberRole`, `OrganizationMembershipInvitation`, `OrganizationSettingVersion`, `OrganizationBranding`, `OrganizationFiscalPreference`, `OrganizationLocale`, `OrganizationOperatingLocation`, `OrganizationCostCenterAssignment`, `OrganizationSubscription`, `OrganizationModuleEntitlement`, `OrganizationLifecycleEvent`, `OrganizationComplianceRegistration`, `OrganizationBankAccountLink`.

Records are pure C# types and do not register EF mappings, queries, use cases or HTTP endpoints. References are identifiers without cross-module navigation.

## 2026-10-13 — Foundation stabilization / ORG-002

The **existing operational legacy** `OrganizationAccessService` now checks organization activity in addition to active membership and role permission on every protected organization operation. For a deactivated organization, **all business/financial reads and mutations are denied**. The only authorized exception is `organizations.reactivate` for its active **Owner**, used solely by `OrganizationService.ActivateAsync`; administrators/integrations do not receive this role permission. Re-activation does not erase existing financial data. New module Domain assembly remains model-only. See `docs/financial-system-of-record.md`.
