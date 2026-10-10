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

## Current-state bearer validation

After normal JWT signature/issuer/audience/lifetime validation, every bearer request verifies that the user still exists and is active. Integration tokens additionally require the exact client/organization identity, an active client and organization, and scopes that are still granted. Deactivation or scope removal therefore affects the next request without waiting for token expiry. Added scopes do not elevate old tokens. Database failures fail the request rather than authorizing from stale state.

This does not introduce access-token session IDs or immediate per-session logout revocation: existing access tokens for an active user retain their normal lifetime after refresh-session revocation. Refresh rotation concurrency is tracked as AUTH-002.

## Atomic refresh rotation

Refresh consumption uses a conditional database update in the same transaction as the replacement insertion. Concurrent use of the same token has one winner; the loser receives the existing authentication error. A failed insertion rolls back consumption. Revocation uses conditional updates so a stale revoke cannot overwrite rotation metadata. If refresh commits before revocation of the old token, the replacement remains valid; logout does not revoke the entire token family or immediately revoke an issued access token.

PostgreSQL regression tests use `FINTROX_TEST_POSTGRES`, pointing to a disposable server with CREATE DATABASE permission. Each test creates and drops its own random `fintrox_auth_test_` database; it never resets the supplied database. CI sets this variable. Without it, the five database tests are explicitly inconclusive.
