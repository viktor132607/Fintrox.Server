# Authentication and authorization

Fintrox uses ASP.NET Core Identity for user accounts and standard JWT bearer access tokens for API authentication.

## Tokens

- Access token lifetime: 15 minutes by default.
- Refresh token lifetime: 30 days.
- Refresh tokens are generated with 64 random bytes.
- PostgreSQL stores only SHA-256 refresh-token hashes.
- Refresh uses rotation: the previous token is revoked and linked to its replacement.
- Sessions can be listed and revoked individually.

Production must provide a strong secret through:

```text
Jwt__SigningKey
```

The API refuses to start when the signing key is shorter than 32 characters.

## Account protection

Passwords require at least 12 characters and must contain uppercase, lowercase, digit and non-alphanumeric characters.

Accounts are locked for 15 minutes after 5 failed password attempts.

ASP.NET Core Identity stores email-confirmation and two-factor state. Mandatory email confirmation is intentionally disabled until a real notification/email delivery workflow is connected; authentication must not pretend an email was delivered when no sender exists.

## Organization roles

Roles are scoped per organization, not globally.

A user can therefore be Owner in one organization and Viewer in another.

Roles:

- Owner
- Administrator
- Accountant
- Operator
- Viewer

The last active Owner cannot be demoted or deactivated.

Non-owner members cannot assign or manage a role equal to or higher than their own role.

## Permissions

Permissions currently include:

- organizations.read
- organizations.manage
- members.manage
- accounting.read
- accounting.write
- accounting.post
- sales.read
- sales.write
- purchases.read
- purchases.write
- payments.read
- payments.write
- reports.read
- integrations.manage

Owner receives all permissions. Other roles receive explicitly mapped subsets in `RolePermissions`.

Permission policies use the authenticated user together with `X-Organization-Id` to resolve organization-scoped access.

## Authentication API

```text
POST   /api/v1/auth/register
POST   /api/v1/auth/login
POST   /api/v1/auth/refresh
POST   /api/v1/auth/revoke
GET    /api/v1/auth/me
GET    /api/v1/auth/sessions
DELETE /api/v1/auth/sessions/{sessionId}
```

## Membership API

```text
GET    /api/v1/organizations/{organizationId}/members
POST   /api/v1/organizations/{organizationId}/members
PUT    /api/v1/organizations/{organizationId}/members/{userId}/role
DELETE /api/v1/organizations/{organizationId}/members/{userId}
```

Creating an organization automatically creates an Owner membership for the authenticated creator.
