# CRM

Status: SCAFFOLD — Architecture created; business logic not implemented.

## Purpose
Manage prospect relationships and the sales pipeline.

## Business Responsibilities
Leads, opportunities and relationship activities; Counterparties owns customer master data and Sales owns quotes/invoices.

## Planned Features
- Leads
- Opportunities
- Activities
- Customer360
- SalesPipeline

## Domain Ownership
Leads, opportunities and relationship activities; Counterparties owns customer master data and Sales owns quotes/invoices.
Reserved PostgreSQL schema: `crm` (not created). No entities or migrations exist yet.

## Dependencies
Planned logical collaborators: Counterparties, Sales.
These are future contract/event relationships, not direct project references.
Domain and Contracts have no references; Application references own Domain/Contracts;
Infrastructure references own Application/Domain/Contracts; Presentation references own Application/Contracts.
The API host references Infrastructure/Presentation for future composition; no services or endpoints are registered.
Foreign data uses IDs/snapshots and approved versioned contracts; no cross-module DbContext, entity navigation or SQL joins.

## Future Integration Events
- `LeadQualifiedV1` — proposed only; payload and delivery semantics to be designed.
- `OpportunityWonV1` — proposed only; payload and delivery semantics to be designed.

## Future Capabilities
- `crm.leads` — reserved, not enabled.
- `crm.opportunities` — reserved, not enabled.
- `crm.activities` — reserved, not enabled.
- `crm.customer360` — reserved, not enabled.
- `crm.sales-pipeline` — reserved, not enabled.

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
