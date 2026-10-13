# Integrations

Status: PARTIAL — Domain entity foundation implemented; persistence, application use cases and API pending.

Owns: ApiClients, ExternalEvents, Idempotency, Inbox, Outbox, Webhooks, AppRegistry, Installations, OrganizationMappings, Activation, ConnectionConfiguration, CredentialLifecycle, ConnectorManagement.
Reserved PostgreSQL schema: `integrations` (not created).
Existing implementation/reference: Integrations; integration schema.

## Boundaries

- Domain: pure model, value objects, invariants and internal domain events.
- Application: feature-oriented use cases; ports in Abstractions. Commands/queries belong under their feature when implemented.
- Contracts: versioned DTOs and integration events; no entities, EF, ASP.NET or implementation dependencies.
- Infrastructure: module-owned context, configurations, migrations, messaging and external adapters.
- Presentation: thin HTTP endpoints and transport mapping; no persistence access.
- Composition: the host explicitly wires Infrastructure and Presentation when implementation is authorized.

Foreign data is referenced by identifiers/snapshots and accessed through approved Contracts.
No cross-module DbContext, repositories, entity navigations, SQL joins, foreign keys or shared transactions in the target architecture.
See [architecture](../../../docs/architecture.md) for the current legacy exceptions and migration gates.

## Implemented entity foundation

`AppRegistration`, `AppVersion`, `AppInstallation`, `ExternalReferenceMapping`.

Models live in this module Domain/Entities directory; foreign objects are referenced by IDs. These are domain types, not registered database tables or endpoints. Existing legacy workflows remain in core projects. See [module progress](../../../docs/ai/MODULE_STATUS.md) for inventory and remaining work.

## Domain expansion — 2026-10-13

Added 24 entities (28 total): `ConnectorDefinition`, `ConnectorVersion`, `ConnectorInstallation`, `ConnectorCredential`, `ConnectorSyncJob`, `ConnectorSyncCheckpoint`, `ConnectorMappingRule`, `ConnectorMappingField`, `WebhookSubscriptionConfig`, `WebhookDeliveryAttempt`, `WebhookDeliveryReceipt`, `IntegrationSecretRotation`, `IntegrationErrorEvent`, `IntegrationQuota`, `ExternalIdentityMapping`, `IntegrationSubscription`, `IntegrationCapabilityGrant`, `IntegrationPermissionScope`, `IntegrationHealthStatus`, `IntegrationAuditRecord`, `IntegrationReplayRequest`, `ExternalDataContract`, `IntegrationRetryPolicy`, `ExternalEventSchema`.

Module-local immutable records, constructor input validation, no references to other Domain assemblies. Persistence, API, security authorizations and implementation remain pending.

## 2026-10-13 — INT-001 live webhook SSRF hardening

The existing **legacy runtime** under `src/Fintrox.Domain/Integrations`, `src/Fintrox.Infrastructure/Integrations` and `src/Fintrox.Api/Integrations` now rejects private/local/metadata webhook targets, validates all DNS answers at socket creation, pins socket connections to validated IP addresses and disables HTTP redirects, proxies and cookies. Existing persisted subscriptions are checked again before delivery; unsafe targets fail permanently. The feature is CI verified in run **38399255676**: 2,891 passing tests, 0 skipped/failed, Release build 0 warnings/errors and EF/Postgres migration checks. Details: [webhook security](../../../docs/webhook-security.md).

**Not migrated into module assemblies:** new Integrations module Domain types are still models; their Infrastructure/Application/Presentation integration, migrations and API are pending. INT-002 worker claim/recovery also remains pending.

## 2026-10-14 — INT-002 webhook lease/recovery (CI verified)

The existing operational webhook worker now acquires a conditional PostgreSQL claim before each HTTP call, using a two-minute lease over the existing next-attempt field, and finishes via compare-and-swap checks on the claimed generation/deadline. Expired leases can be recovered by other server instances; stale responses cannot overwrite newer delivery results. Terminal failures and failure evidence commit in the same transaction; the sixth expired attempt ends without a seventh HTTP send. Four real PostgreSQL tests added. **CI 38475619708 VERIFIED: 2,906 tests passed, 0 errors/warnings; EF/PostgreSQL checks passed.** [Details](../../../docs/webhook-lease-recovery.md). The new module persistence and API are not yet migrated.
