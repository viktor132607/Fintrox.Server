# Commerce

Status: SCAFFOLD — Architecture created; business logic not implemented.

## Purpose
Coordinate storefront and point-of-sale commerce.

## Business Responsibilities
Channel listings, channel pricing/promotions and commerce intake; Inventory owns product/stock master data, Sales owns financial documents, Integrations owns external installations.

## Planned Features
- Catalog
- Pricing
- Promotions
- CommerceOrders
- StorefrontIntegrations
- PointOfSale

## Domain Ownership
Channel listings, channel pricing/promotions and commerce intake; Inventory owns product/stock master data, Sales owns financial documents, Integrations owns external installations.
Reserved PostgreSQL schema: `commerce` (not created). No entities or migrations exist yet.

## Dependencies
Planned logical collaborators: Sales, OrderManagement, Integrations.
These are future contract/event relationships, not direct project references.
Domain and Contracts have no references; Application references own Domain/Contracts;
Infrastructure references own Application/Domain/Contracts; Presentation references own Application/Contracts.
The API host references Infrastructure/Presentation for future composition; no services or endpoints are registered.
Foreign data uses IDs/snapshots and approved versioned contracts; no cross-module DbContext, entity navigation or SQL joins.

## Future Integration Events
- `CommerceOrderReceivedV1` — proposed only; payload and delivery semantics to be designed.
- `PromotionActivatedV1` — proposed only; payload and delivery semantics to be designed.

## Future Capabilities
- `commerce.catalog` — reserved, not enabled.
- `commerce.pricing` — reserved, not enabled.
- `commerce.promotions` — reserved, not enabled.
- `commerce.commerce-orders` — reserved, not enabled.
- `commerce.storefront-integrations` — reserved, not enabled.
- `commerce.point-of-sale` — reserved, not enabled.

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
