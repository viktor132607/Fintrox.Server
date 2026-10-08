# WarehouseManagement

Status: SCAFFOLD — Architecture created; business logic not implemented.

## Purpose
Coordinate physical warehouse execution.

## Business Responsibilities
Warehouse tasks, bins and execution state; Inventory owns stock quantities and valuation.

## Planned Features
- Receiving
- Putaway
- Picking
- Packing
- Shipping
- CycleCounting

## Domain Ownership
Warehouse tasks, bins and execution state; Inventory owns stock quantities and valuation.
Reserved PostgreSQL schema: `warehouse_management` (not created). No entities or migrations exist yet.

## Dependencies
Planned logical collaborators: Inventory, OrderManagement.
These are future contract/event relationships, not direct project references.
Domain and Contracts have no references; Application references own Domain/Contracts;
Infrastructure references own Application/Domain/Contracts; Presentation references own Application/Contracts.
The API host references Infrastructure/Presentation for future composition; no services or endpoints are registered.
Foreign data uses IDs/snapshots and approved versioned contracts; no cross-module DbContext, entity navigation or SQL joins.

## Future Integration Events
- `GoodsReceivedV1` — proposed only; payload and delivery semantics to be designed.
- `ShipmentDispatchedV1` — proposed only; payload and delivery semantics to be designed.

## Future Capabilities
- `warehouse-management.receiving` — reserved, not enabled.
- `warehouse-management.putaway` — reserved, not enabled.
- `warehouse-management.picking` — reserved, not enabled.
- `warehouse-management.packing` — reserved, not enabled.
- `warehouse-management.shipping` — reserved, not enabled.
- `warehouse-management.cycle-counting` — reserved, not enabled.

## Implementation Status
Five real .NET projects and feature folders exist. Business rules, DTOs, services,
endpoints, migrations and tests remain unimplemented. Test folders are placeholders.
Framework/compiler settings inherit from the repository Directory.Build.props.

## Roadmap
1. Specify use cases, tenant boundaries, permissions and acceptance tests.
2. Define versioned contracts and implement domain/application behavior.
3. Add module-owned persistence/adapters and thin endpoints; register the implemented module.
4. Verify unit, PostgreSQL integration and contract tests before activation or frontend integration.

See [architecture](../../../docs/architecture.md) and [master roadmap](../../../docs/ai/MASTER_ROADMAP.md).
