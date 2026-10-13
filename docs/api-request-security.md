# API-001 — browser CORS and authentication throttling

## Implementation (2026-10-14)

The active ASP.NET Core Web API configures a named `FintroxFrontend` CORS policy through
`src/Fintrox.Api/Security/ApiRequestSecurity.cs` and applies it after routing,
before authentication and authorization. Allowed origins are read only from
`Cors:AllowedOrigins`; an empty list is the safe default. Entries must be exact
HTTP(S) origins (scheme, host and optional port) with no slash, path, query,
wildcard, credentials or fragment. Malformed entries fail host startup.
No wildcard origin, reflected arbitrary origin or credential-sharing policy is enabled.

For development, explicitly configure both needed localhost variants through
`Cors__AllowedOrigins__0=http://localhost:3000` and
`Cors__AllowedOrigins__1=http://127.0.0.1:3000` (or use a local noncommitted
appsettings override). Production must set its actual HTTPS Next.js origin, e.g.
`Cors__AllowedOrigins__0=https://app.example.com`. Backend-to-backend calls
do not require CORS. CORS is a browser boundary, not authentication or
authorization; the existing JWT/permission/tenant checks remain in force.

Named fixed-window limiter policies apply directly to anonymous endpoints:

| Endpoint | Limit | Window |
| --- | ---: | --- |
| POST /api/v1/auth/register | 5 | 1 hour |
| POST /api/v1/auth/login | 10 | 1 minute |
| POST /api/v1/auth/refresh | 30 | 1 minute |
| POST /api/v1/auth/revoke | 30 | 1 minute |
| POST /api/v1/integrations/token | 20 | 1 minute |

Each named policy is partitioned by ASP.NET Core's server-observed
`HttpContext.Connection.RemoteIpAddress`, normalized to IPv6. There is no
client-controlled `X-Forwarded-For` or tenant/user/request-body partition bypass.
Requests exceeding a policy's limit receive HTTP 429 without a queue.
Other routes, including system health and financial operations, are not throttled
by these authentication-specific policies.

## Regression coverage

`tests/Fintrox.Domain.Tests/Api/ApiRequestSecurityHttpTests.cs` adds six
WebApplicationFactory HTTP tests: explicit-origin preflight and normal response;
untrusted-origin preflight and response rejection; default deny without configured
origins; login saturation (429) without limiting unrelated endpoints; independent
registration/integration-token windows; and rejection of spoofed forwarding headers.
Malformed JSON is used to avoid database mutation while executing actual ASP.NET
Core middleware and routing. CI/build verification is tracked in
`docs/ai/VALIDATION.md`.

## Operational limitations

- Built-in in-memory rate-limiter counters are **per server instance**, not
  distributed across a fleet or durable over restarts. Production multi-instance
  deployments should introduce a shared/store-backed edge rate limit.
- Users behind the same NAT/shared IP consume the same quota. Limits are
  adjustable in `ApiRequestSecurity.cs`; choose production thresholds based on
  expected traffic, risk and load tests.
- If running behind a reverse proxy, configure ASP.NET Core Forwarded Headers
  with an **explicit trusted proxy/network list** before relying on the client IP.
  Never unconditionally trust a public `X-Forwarded-For` value. Until then, all
  clients behind the proxy may share its observed remote address.
- The integration-token quota limits attempts per IP but does not replace
  secret revocation, tenant authorization or credential monitoring.
- No database migration or financial/domain schema change is part of API-001.
  The 936 modular Domain entities remain separate from their pending EF/API
  migrations.
