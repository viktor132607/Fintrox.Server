# SupplyChain

Status: PARTIAL — Domain entity foundation implemented; persistence, application use cases and API pending.

## Purpose
Coordinate demand, supply and replenishment plans.

## Business Responsibilities
Planning recommendations and supplier collaboration; Purchases owns purchase documents and Inventory owns stock.

## Planned Features
- DemandPlanning
- SupplyPlanning
- ProcurementPlanning
- Replenishment
- SupplierCollaboration

## Domain Ownership
Planning recommendations and supplier collaboration; Purchases owns purchase documents and Inventory owns stock.
Reserved PostgreSQL schema: `supply_chain` (not created). Entity types exist; module migrations are not implemented.

## Dependencies
Planned logical collaborators: Purchases, Inventory.
These are future contract/event relationships, not direct project references.
Domain and Contracts have no references; Application references own Domain/Contracts;
Infrastructure references own Application/Domain/Contracts; Presentation references own Application/Contracts.
The API host references Infrastructure/Presentation for future composition; no services or endpoints are registered.
Foreign data uses IDs/snapshots and approved versioned contracts; no cross-module DbContext, entity navigation or SQL joins.

## Future Integration Events
- `ReplenishmentRequestedV1` — proposed only; payload and delivery semantics to be designed.
- `SupplyPlanApprovedV1` — proposed only; payload and delivery semantics to be designed.

## Future Capabilities
- `supply-chain.demand-planning` — reserved, not enabled.
- `supply-chain.supply-planning` — reserved, not enabled.
- `supply-chain.procurement-planning` — reserved, not enabled.
- `supply-chain.replenishment` — reserved, not enabled.
- `supply-chain.supplier-collaboration` — reserved, not enabled.

## Implementation Status
Five .NET projects and feature folders exist. Domain constructors validate their inputs and are covered by the shared executable test project. DTOs, services, endpoints and migrations remain unimplemented.
Framework/compiler settings inherit from the repository Directory.Build.props.

## Roadmap
1. Specify use cases, tenant boundaries, permissions and acceptance tests.
2. Define versioned contracts and implement domain/application behavior.
3. Add module-owned persistence/adapters and thin endpoints; register the implemented module.
4. Verify unit, PostgreSQL integration and contract tests before activation or frontend integration.

See [architecture](../../../docs/architecture.md) and [master roadmap](../../../docs/ai/MASTER_ROADMAP.md).

## Implemented entity foundation

`DemandForecast`, `SupplyPlan`, `ProcurementPlan`, `ReplenishmentPolicy`, `SupplierCommitment`.

Models live in this module Domain/Entities directory; foreign objects are referenced by IDs. These are domain types, not registered database tables or endpoints. Existing legacy workflows remain in core projects. See [module progress](../../../docs/ai/MODULE_STATUS.md) for inventory and remaining work.

## Domain expansion — 2026-10-13

Added 21 immutable, constructor-validated entity classes (26 total): `DemandPlanVersion`, `DemandPlanLine`, `DemandForecastFactor`, `SupplyPlanVersion`, `SupplyPlanLine`, `SupplyDemandPegging`, `SupplierCapacityPlan`, `SupplierCapacityReservation`, `SupplierLeadTime`, `SupplierCollaborationMessage`, `SupplierShipmentNotice`, `SupplierShipmentNoticeLine`, `ProcurementRecommendation`, `ReplenishmentExecution`, `ReplenishmentProposal`, `SafetyStockRecommendation`, `SupplyChainRisk`, `SupplyChainDisruption`, `SupplyChainTransportLeg`, `SupplyChainShipmentMilestone`, `SupplyChainInventoryProjection`.

No EF migrations, API endpoints, domain services or external legal/business rules are implemented in this model-only commit.
