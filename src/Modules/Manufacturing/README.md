# Manufacturing

Status: PARTIAL — Domain entity foundation implemented; persistence, application use cases and API pending.

## Purpose
Plan and execute production.

## Business Responsibilities
Bills of materials, routings and work orders; Inventory owns stock and Accounting owns ledger posting.

## Planned Features
- BillOfMaterials
- WorkOrders
- Routings
- ProductionPlanning
- ProductionCosting

## Domain Ownership
Bills of materials, routings and work orders; Inventory owns stock and Accounting owns ledger posting.
Reserved PostgreSQL schema: `manufacturing` (not created). Entity types exist; module migrations are not implemented.

## Dependencies
Planned logical collaborators: Inventory, Accounting, SupplyChain.
These are future contract/event relationships, not direct project references.
Domain and Contracts have no references; Application references own Domain/Contracts;
Infrastructure references own Application/Domain/Contracts; Presentation references own Application/Contracts.
The API host references Infrastructure/Presentation for future composition; no services or endpoints are registered.
Foreign data uses IDs/snapshots and approved versioned contracts; no cross-module DbContext, entity navigation or SQL joins.

## Future Integration Events
- `WorkOrderReleasedV1` — proposed only; payload and delivery semantics to be designed.
- `ProductionCompletedV1` — proposed only; payload and delivery semantics to be designed.

## Future Capabilities
- `manufacturing.bill-of-materials` — reserved, not enabled.
- `manufacturing.work-orders` — reserved, not enabled.
- `manufacturing.routings` — reserved, not enabled.
- `manufacturing.production-planning` — reserved, not enabled.
- `manufacturing.production-costing` — reserved, not enabled.

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

`BillOfMaterials`, `BillOfMaterialsLine`, `ProductionRouting`, `RoutingOperation`, `WorkCenter`, `WorkOrder`, `ProductionOutput`.

Models live in this module Domain/Entities directory; foreign objects are referenced by IDs. These are domain types, not registered database tables or endpoints. Existing legacy workflows remain in core projects. See [module progress](../../../docs/ai/MODULE_STATUS.md) for inventory and remaining work.

## Domain expansion — 2026-10-13

Added 24 validated Domain entities (31 total): `BomVersion`, `BomComponentAlternative`, `ProductionWorkOrderLine`, `ProductionMaterialRequirement`, `ProductionMaterialIssue`, `ProductionMaterialReturn`, `ProductionLot`, `ProductionOperationExecution`, `MachineAsset`, `MachineDowntime`, `ProductionShift`, `WorkCenterCapacity`, `ProductionSchedule`, `ProductionScheduleException`, `ProductionQualityCheck`, `ProductionScrapRecord`, `ProductionByproduct`, `ProductionYieldRecord`, `ProductionCostPool`, `ProductionCostAllocation`, `WorkInProgressBalance`, `ProductionVariance`, `ProductionCompletion`, `ManufacturingTraceabilityLink`.

Records are pure C# types and do not register EF mappings, queries, use cases or HTTP endpoints. References are identifiers without cross-module navigation.
