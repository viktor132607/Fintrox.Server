# QualityManagement

Status: PARTIAL — Domain entity foundation implemented; persistence, application use cases and API pending.

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
Reserved PostgreSQL schema: `quality_management` (not created). Entity types exist; module migrations are not implemented.

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
Five .NET projects and feature folders exist. Domain constructors validate their inputs and are covered by the shared executable test project. DTOs, services, endpoints and migrations remain unimplemented.
Framework/compiler settings inherit from the repository Directory.Build.props.

## Roadmap
1. Specify use cases, tenant boundaries, permissions and acceptance tests.
2. Define versioned contracts and implement domain/application behavior.
3. Add module-owned persistence/adapters and thin endpoints; register the implemented module.
4. Verify unit, PostgreSQL integration and contract tests before activation or frontend integration.

See [architecture](../../../docs/architecture.md) and [master roadmap](../../../docs/ai/MASTER_ROADMAP.md).

## Implemented entity foundation

`InspectionPlan`, `InspectionCriterion`, `QualityInspection`, `InspectionResult`, `Nonconformity`, `CorrectiveAction`, `QualityHold`.

Models live in this module Domain/Entities directory; foreign objects are referenced by IDs. These are domain types, not registered database tables or endpoints. Existing legacy workflows remain in core projects. See [module progress](../../../docs/ai/MODULE_STATUS.md) for inventory and remaining work.

## Domain expansion — 2026-10-13

Added 21 constructor-validated models (28 total): `QualityStandard`, `QualityStandardRequirement`, `InspectionSamplingPlan`, `InspectionBatch`, `InspectionSample`, `InspectionMeasurement`, `QualityRelease`, `QualityDeviation`, `QualityDefectCategory`, `QualityDefect`, `QualityContainmentAction`, `QualityRootCauseAnalysis`, `CorrectiveActionTask`, `PreventiveAction`, `QualityEffectivenessCheck`, `QualitySupplierClaim`, `QualityCustomerComplaint`, `QualityRecall`, `QualityTraceabilityRecord`, `QualityCertificate`, `QualityEquipmentCalibration`.

Domain only. Real transactions, reporting execution, database schema, localization, integration, authorizations and APIs are not implemented by these classes.
