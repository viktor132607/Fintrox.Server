# QualityManagement

Status: SCAFFOLD — Architecture created; business logic not implemented.

## Purpose
Manage quality checks and corrective actions.

## Business Responsibilities
Inspection results, nonconformities and quality traceability; Manufacturing owns production and Inventory owns stock.

## Planned Features
- Inspections
- QualityControl
- Nonconformities
- CorrectiveActions
- Traceability

## Domain Ownership
Inspection results, nonconformities and quality traceability; Manufacturing owns production and Inventory owns stock.
Reserved PostgreSQL schema: `quality_management` (not created). No entities or migrations exist yet.

## Dependencies
Planned logical collaborators: Manufacturing, Inventory.
These are future contract/event relationships, not direct project references.
Domain and Contracts have no references; Application references own Domain/Contracts;
Infrastructure references own Application/Domain/Contracts; Presentation references own Application/Contracts.
The API host references Infrastructure/Presentation for future composition; no services or endpoints are registered.
Foreign data uses IDs/snapshots and approved versioned contracts; no cross-module DbContext, entity navigation or SQL joins.

## Future Integration Events
- `InspectionCompletedV1` — proposed only; payload and delivery semantics to be designed.
- `NonconformityRaisedV1` — proposed only; payload and delivery semantics to be designed.

## Future Capabilities
- `quality-management.inspections` — reserved, not enabled.
- `quality-management.quality-control` — reserved, not enabled.
- `quality-management.nonconformities` — reserved, not enabled.
- `quality-management.corrective-actions` — reserved, not enabled.
- `quality-management.traceability` — reserved, not enabled.

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
