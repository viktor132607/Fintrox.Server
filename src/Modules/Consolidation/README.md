# Consolidation

Status: PARTIAL — Domain entity foundation implemented; persistence, application use cases and API pending.

Owns: Groups, Intercompany, Eliminations, ConsolidatedStatements.
Reserved PostgreSQL schema: `consolidation` (not created).
Existing implementation/reference: Not implemented.

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

`CompanyGroup`, `GroupCompany`, `ConsolidationPeriod`, `EliminationEntry`.

Models live in this module Domain/Entities directory; foreign objects are referenced by IDs. These are domain types, not registered database tables or endpoints. Existing legacy workflows remain in core projects. See [module progress](../../../docs/ai/MODULE_STATUS.md) for inventory and remaining work.

## Domain expansion — 2026-10-13

Added 18 validated entities (22 total): `IntercompanyPartnerLink`, `IntercompanyTransaction`, `IntercompanyReconciliation`, `ConsolidationRule`, `ConsolidationRun`, `ConsolidationWorksheet`, `ConsolidationWorksheetLine`, `ConsolidationCurrencyTranslation`, `ConsolidationExchangeRateSet`, `MinorityInterest`, `OwnershipChange`, `ConsolidationAdjustment`, `ConsolidationJournal`, `ConsolidationDisclosure`, `ConsolidatedStatement`, `ConsolidatedStatementLine`, `GroupAccountingPolicy`, `ConsolidationReportSnapshot`.

Entity-only implementation; persistence, versioned DTOs, application logic, endpoints, tests with PostgreSQL and frontend are not implemented by this commit.
