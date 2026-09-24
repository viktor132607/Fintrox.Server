# Multi-tenancy

Fintrox is organization-scoped by design.

## Tenant boundary

Every future business entity that belongs to a legal/accounting entity must implement:

```csharp
IOrganizationScopedEntity
```

and expose a non-empty `OrganizationId`.

`FintroxDbContext` rejects added or modified organization-scoped entities whose `OrganizationId` is empty.

## Organization context

HTTP clients send the active organization through:

```text
X-Organization-Id: <organization-guid>
```

`ICurrentOrganization` is the application-layer abstraction. The API currently resolves it from the request header.

Authentication and membership authorization are intentionally introduced in the next phase. After Identity is added, the header will identify the requested organization while authorization will verify that the authenticated user is a member with sufficient permissions.

## Organization fields

An organization stores:

- display name;
- optional legal name;
- stable unique slug;
- ISO-style two-letter country code;
- three-letter base currency code;
- IANA/host time-zone identifier;
- optional registration number;
- optional VAT number;
- active/inactive state;
- UTC creation/update timestamps.

The slug is immutable after creation and is intended to be a stable external identifier.

## API

```text
GET    /api/v1/organizations
GET    /api/v1/organizations/{id}
POST   /api/v1/organizations
PUT    /api/v1/organizations/{id}
DELETE /api/v1/organizations/{id}
POST   /api/v1/organizations/{id}/activate
```

Until phase 6 adds authentication, these organization-management endpoints are not production-ready and must not be exposed publicly.
