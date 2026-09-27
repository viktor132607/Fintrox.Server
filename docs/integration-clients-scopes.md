# Integration clients and scopes

Phase 19 adds machine-to-machine authentication for other Fintrox-connected applications.

## Integration clients

An integration client belongs to exactly one organization and has:

- public `ClientId`;
- human-readable name;
- active/inactive state;
- explicit scopes;
- secret prefix for operator identification;
- secret rotation timestamp;
- last-used timestamp.

The full client secret is returned only when the client is created or its secret is rotated.

Fintrox stores only a SHA-256 hash of the high-entropy generated secret. Client secrets are generated from 256 bits of cryptographic randomness.

## Token exchange

```http
POST /api/v1/integrations/token
Content-Type: application/json

{
  "grantType": "client_credentials",
  "clientId": "fic_...",
  "clientSecret": "fis_..."
}
```

The response contains a short-lived Bearer JWT and no refresh token.

Integration JWT claims include:

- `actor_type=integration`;
- internal subject id;
- public `client_id`;
- fixed `org_id`;
- one `scope` claim per granted permission.

## Tenant isolation

Machine tokens are pinned to the organization stored on the integration client.

For integration requests:

- `X-Organization-Id` may be omitted;
- if it is present, it must match the token's `org_id`;
- a mismatched organization header causes authorization to fail.

An integration token therefore cannot be reused against another tenant.

## Allowed machine scopes

Integration clients can receive only business/API permissions:

- `accounting.read`
- `accounting.write`
- `accounting.post`
- `counterparties.read`
- `counterparties.write`
- `tax.read`
- `tax.write`
- `currencies.read`
- `currencies.write`
- `sales.read`
- `sales.write`
- `purchases.read`
- `purchases.write`
- `payments.read`
- `payments.write`
- `reports.read`

Machine clients cannot receive organization administration, member administration, or `integrations.manage`.

## Management API

Management requires a human user with `integrations.manage` in the selected organization.

```text
GET    /api/v1/integrations/clients
GET    /api/v1/integrations/clients/{id}
POST   /api/v1/integrations/clients
PUT    /api/v1/integrations/clients/{id}
POST   /api/v1/integrations/clients/{id}/rotate-secret
DELETE /api/v1/integrations/clients/{id}
POST   /api/v1/integrations/clients/{id}/activate
```

Deactivation blocks all future token exchanges. Existing JWTs remain valid until their short expiration window ends.

Rotating a secret invalidates the old secret immediately for future token exchanges.

## Configuration

`Jwt:IntegrationAccessTokenMinutes` controls machine-token lifetime and defaults to 15 minutes.
