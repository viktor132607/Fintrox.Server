# ServiceManagement

Status: PARTIAL — Domain entity foundation implemented; persistence, application use cases and API pending.

## Purpose
Coordinate customer support and service delivery.

## Business Responsibilities
Tickets, service levels and field-service/maintenance work; Counterparties owns customer records and CRM owns opportunities.

## Planned Features
- SupportTickets
- ServiceLevels
- FieldService
- Maintenance
- CustomerSupport

## Domain Ownership
Tickets, service levels and field-service/maintenance work; Counterparties owns customer records and CRM owns opportunities.
Reserved PostgreSQL schema: `service_management` (not created). Entity types exist; module migrations are not implemented.

## Dependencies
Planned logical collaborators: CRM, Counterparties.
These are future contract/event relationships, not direct project references.
Domain and Contracts have no references; Application references own Domain/Contracts;
Infrastructure references own Application/Domain/Contracts; Presentation references own Application/Contracts.
The API host references Infrastructure/Presentation for future composition; no services or endpoints are registered.
Foreign data uses IDs/snapshots and approved versioned contracts; no cross-module DbContext, entity navigation or SQL joins.

## Future Integration Events
- `TicketResolvedV1` — proposed only; payload and delivery semantics to be designed.
- `ServiceLevelBreachedV1` — proposed only; payload and delivery semantics to be designed.

## Future Capabilities
- `service-management.support-tickets` — reserved, not enabled.
- `service-management.service-levels` — reserved, not enabled.
- `service-management.field-service` — reserved, not enabled.
- `service-management.maintenance` — reserved, not enabled.
- `service-management.customer-support` — reserved, not enabled.

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

`SupportTicket`, `ServiceLevelAgreement`, `FieldServiceOrder`, `MaintenancePlan`.

Models live in this module Domain/Entities directory; foreign objects are referenced by IDs. These are domain types, not registered database tables or endpoints. Existing legacy workflows remain in core projects. See [module progress](../../../docs/ai/MODULE_STATUS.md) for inventory and remaining work.
