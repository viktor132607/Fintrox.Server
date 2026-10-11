# Capabilities

Status: PARTIAL — Domain entity foundation implemented; persistence, application use cases and API pending.
Reserved schema: `capabilities`; no schema, DbContext or tables created yet.

Use the same five-layer dependency direction as existing modules. No cross-module references are introduced.
See [activation and experience boundaries](../../../docs/activation-experience.md) for ownership and future contracts.

## Implemented entity foundation

`CapabilityDefinition`, `CapabilityDependency`, `OrganizationModuleInstallation`, `OrganizationCapabilityActivation`, `UserCapabilityEntitlement`.

Models live in this module Domain/Entities directory; foreign objects are referenced by IDs. These are domain types, not registered database tables or endpoints. Existing legacy workflows remain in core projects. See [module progress](../../../docs/ai/MODULE_STATUS.md) for inventory and remaining work.
