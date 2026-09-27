# Activatable integrations and configurable experience

Status: structure only. No services, endpoints, database migrations, grants, widgets,
profile defaults, SDK packages or rendering logic are implemented by this change.

## Ownership

| Boundary | Owns | Does not own |
|---|---|---|
| Integrations | External app registry, tenant-bound installations, external organization mappings, configuration, credential lifecycle, connectors, external API/event transport | Ledger rules, user layout, global authorization |
| Capabilities | Stable feature IDs, dependencies, module/capability activation, configuration and availability | Identity permissions or UI positioning |
| Experience | Presets, templates, widget metadata, per-user/per-organization layouts, navigation, preferences, configuration versions | Financial operations or authorization |
| Identity | Users, sessions, permissions, membership-aware authorization | Feature activation and profile selection |
| Business modules | Their use cases and the capabilities they expose | External client-specific logic or UI engine |

Keep the existing 21 boundaries. The two platform modules use the same five assembly
layers, solution membership and host references. They register no runtime services.
No cross-module project dependencies are added at this stage. Later dependencies must
use explicitly reviewed Contracts and be added to the architecture policy individually.

## External applications

An external application registers once; an installation is a separate connection for
one organization. An application may have multiple installations. External tenant IDs
must be explicitly mapped to authorized Fintrox organization IDs, never trusted from
an unverified event payload. Credential references and configuration are installation scoped.

Reserve lifecycle use cases for registration, connecting, activating, suspending,
deactivating, revoking credentials and reconnecting. Activation is not credential issuance;
configuration completion and permission checks are prerequisites. Disabling an installation
stops new submissions/dispatch according to an explicit policy. Durable inbox/outbox work
must be drained or paused predictably; do not silently discard accepted financial events.
Reactivation must not replay already completed operations. Revocation must invalidate access
independently of any visible UI switch. No financial or audit history is deleted by deactivation.

Integrations reuses existing clients/scopes, idempotency, inbox/outbox, signed webhooks,
failure records and retry foundations through migration adapters later. No duplicate
implementations or foreign executable plugins are introduced. SDK folders reserve .NET
and TypeScript consumers of versioned API contracts; they are not usable SDKs yet.

## Capabilities

`architecture/experience-integration.json` assigns every catalog feature a reserved,
stable ID. These are design-time definitions, not effective runtime entitlements.
Dependencies, incompatible configurations and configuration schemas belong to Capabilities.
Feature activation is organization scoped; installation grants may only narrow that scope.
Cycles and unavailable prerequisites must eventually be rejected before activation.

Available operation = organization feature activation AND caller authorization AND,
for external clients, active installation and approved scopes. Enforce this in the backend.
UI visibility cannot make an unavailable operation executable. An Expert profile is not
an administrator role. Disabling a capability must account for dependent features and
in-flight operations, and must preserve posted history.

## Experience

Exactly three base presets: `simple`, `accountant`, `expert`. Accountant is the canonical
identifier for the earlier mid/Professional description. A saved custom layout is an
override of one preset, not an additional security tier. One application and one set of
business components serve all three presets; no page or business-rule duplication.

Users may add/remove any available individual feature, widgets, navigation entries and
sections, not just entire modules. Reserve ordering, region placement, sizing, device
variants, save, reset and template switching in the layout model. Feature IDs and widget
IDs are distinct: one feature may provide several UI components or no widget at all.
Do not automatically generate a widget for every feature. The widget catalog and default
widget lists remain empty until actual UI functionality is implemented.

Persistence key includes organization, user, profile, layout ID and configuration version.
Organization defaults and personal overrides are separate. Reset removes only the selected
override; switching profiles preserves that profile's previous personal override and uses
its base template if none exists. Templates are immutable/versioned, personal edits use
optimistic concurrency. Future migrations upgrade saved configurations without losing
unknown or retired entries; unavailable items are omitted from effective rendering while
saved choices remain recoverable. Permission changes are re-evaluated on load and action.

Configuration stores allowlisted IDs and validated presentation settings, never scripts,
arbitrary component imports, SQL, secrets or tokens. Backend owns saved preferences and
validates scope; FE owns rendering and editing. No financial calculations belong in FE.

## FE dependency direction

`app` composes screens via `composition/experience`. Composition can connect public module
entry points to the generic `experience` layer. `experience` only uses itself and `shared`;
business modules only use themselves and `shared`. Shared never imports upward. Public
cross-layer contracts belong in shared/types; generated API contracts remain shared/api/generated.
There is no second frontend, no runtime registration or hidden database access.

## Future contracts and tests

Reserve Contracts/Api/V1 and Contracts/Events/V1 for app, installation, activation,
availability, profile, template, layout and preference contracts. Reference organization,
actor, correlation/causation IDs and contract versions without exporting EF entities.
Security and data ownership need tenant-isolation tests; activation needs dependency and
revocation tests; layouts need persistence, profile-switch/reset, compatibility and permission
intersection tests. These are future behavior tests; structural checks run now in CI.

## Extraction

Each module retains its own future schema and migration ownership. Extraction swaps
contract adapters and hosting, not FE business components. No gateway, message broker,
new authentication scheme or cross-module database transaction is deployed by this change.
