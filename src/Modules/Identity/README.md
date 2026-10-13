# Identity

Status: PARTIAL — Domain entity foundation implemented; persistence, application use cases and API pending.

Owns: Users, Sessions, Roles, Permissions.
Reserved PostgreSQL schema: `identity` (not created).
Existing implementation/reference: Identity, Authentication, Authorization.

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

`OrganizationInvitation`, `RoleDefinition`, `PermissionDefinition`, `RolePermissionGrant`, `ServiceAccount`.

Models live in this module Domain/Entities directory; foreign objects are referenced by IDs. These are domain types, not registered database tables or endpoints. Existing legacy workflows remain in core projects. See [module progress](../../../docs/ai/MODULE_STATUS.md) for inventory and remaining work.

## Domain expansion — 2026-10-13

Added 22 entities (27 total): `UserProfile`, `UserEmailAddress`, `UserPhoneNumber`, `UserMfaFactor`, `UserMfaRecoveryCode`, `UserPasswordHistory`, `UserSecurityEvent`, `UserTrustedDevice`, `UserApiToken`, `UserConsent`, `RoleAssignment`, `PermissionPolicy`, `PermissionPolicyRule`, `RoleScopeAssignment`, `InvitationAcceptance`, `ServiceAccountCredential`, `IdentityVerificationRequest`, `LoginChallenge`, `SessionSecurityContext`, `AccountLockout`, `OrganizationSsoConfiguration`, `IdentityProviderMapping`.

Module-local immutable records, constructor input validation, no references to other Domain assemblies. Persistence, API, security authorizations and implementation remain pending.

## Finance-wide authorization foundation — 2026-10-13

Every financial read, write, authorization, posting and reversal requires current organization/actor permissions. The currently operational legacy `OrganizationAccessService` now denies inactive-tenant business permission checks; a separate owner-only `organizations.reactivate` path preserves restoration. Full Identity module persistence and finer-grained financial approvals are still pending. See [financial system-of-record contract](../../../docs/financial-system-of-record.md).
