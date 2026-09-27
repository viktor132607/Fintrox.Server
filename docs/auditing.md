# Common domain and auditing foundation

All new business entities should use the common domain base types instead of reimplementing identifiers, organization scope or audit metadata.

## Base types

`Entity`

- owns the `Guid Id`;
- rejects empty IDs when constructed explicitly.

`AuditableEntity`

- derives from `Entity`;
- stores `CreatedAtUtc`;
- stores `CreatedByUserId`;
- stores `UpdatedAtUtc`;
- stores `UpdatedByUserId`;
- audit metadata is updated automatically by the persistence pipeline.

`OrganizationScopedAuditableEntity`

- derives from `AuditableEntity`;
- implements `IOrganizationScopedEntity`;
- owns the mandatory `OrganizationId`;
- rejects an empty organization ID.

`IAggregateRoot`

- marker for aggregate roots;
- `Organization` is currently the first aggregate using it.

## Current migration

The current `Organization` and `OrganizationMembership` entities were migrated onto the common base classes.

Their existing timestamps remain intact and the model now also stores the creating/updating user IDs.

## Audit log

Every added, modified or deleted `IAuditableEntity` is captured automatically by `AuditSaveChangesInterceptor`.

The audit record is inserted in the same EF Core `SaveChanges` transaction as the business change.

Audit records contain:

- organization ID when available;
- actor user ID when authenticated;
- entity type;
- entity ID;
- action: Created, Updated or Deleted;
- JSON property changes;
- request correlation ID;
- UTC occurrence timestamp.

The audit table is:

```text
audit.audit_log
```

Indexes support organization/time, user/time and entity-history lookups.

## Immutability

Audit records are append-only.

`FintroxDbContext` rejects modification or deletion of tracked audit records.

Audit rows intentionally do not have cascading foreign keys to organizations or users. Historical evidence therefore survives later deactivation or deletion workflows.

## Sensitive values

Property names containing the following fragments are redacted before serialization:

- password
- token
- secret
- hash
- securitystamp

The audit pipeline should never be used as a secret store.

## Request actor

The API provides `HttpAuditContext`, which resolves:

- the authenticated user;
- the active organization;
- the ASP.NET Core request trace identifier.

For organization-scoped entities, the entity's own `OrganizationId` is authoritative. For the Organization aggregate itself, its own `Id` is used.

## Persistence lifetime

The DbContext uses the regular scoped `AddDbContext` lifetime instead of DbContext pooling.

This is intentional because the audit interceptor depends on request-scoped actor context. A pooled DbContext must not retain request-specific services across requests.
