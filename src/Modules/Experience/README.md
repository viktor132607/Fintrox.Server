# Experience

Status: PARTIAL — Domain entity foundation implemented; persistence, application use cases and API pending.
Reserved schema: `experience`; no schema, DbContext or tables created yet.

Use the same five-layer dependency direction as existing modules. No cross-module references are introduced.
See [activation and experience boundaries](../../../docs/activation-experience.md) for ownership and future contracts.

## Implemented entity foundation

`ExperienceProfile`, `UserExperiencePreference`, `DashboardWidget`, `SavedView`, `NavigationPreference`.

Models live in this module Domain/Entities directory; foreign objects are referenced by IDs. These are domain types, not registered database tables or endpoints. Existing legacy workflows remain in core projects. See [module progress](../../../docs/ai/MODULE_STATUS.md) for inventory and remaining work.

## Domain expansion — 2026-10-13

Added 18 entity classes (23 total): `LayoutTemplate`, `LayoutTemplateVersion`, `UserLayout`, `UserLayoutSection`, `UserLayoutWidget`, `WidgetCatalogEntry`, `WidgetDataBinding`, `NavigationItem`, `NavigationItemPreference`, `DashboardTemplate`, `DashboardSection`, `DashboardAssignment`, `UserFeatureVisibility`, `UserSavedFilter`, `UserTableColumnPreference`, `ExperienceAccessPolicy`, `ExperienceConfigurationVersion`, `UserLocalePreference`.

Pure Domain models with immutable properties and constructor validation. No EF tables, DTOs, API routes or production business flows were added.
