# Outbound webhook destination security — INT-001

**Implemented and CI-verified on 2026-10-13** in the operational legacy Integrations runtime. GitHub Actions [38399255676](https://github.com/viktor132607/Fintrox.Server/actions/runs/38399255676): Release build with **0 warnings / 0 errors**; **2,891 tests passed, 0 failed/skipped**; EF model snapshot and PostgreSQL migration checks passed. The independent Docker publish job does not affect these assertions.

## Threat and boundaries

Organizations can register HTTP(S) webhooks, so an attacker could try to make the Fintrox service call its loopback, VPC/private addresses, cloud metadata endpoints, Kubernetes/Docker service-discovery addresses, or a public hostname that resolves to a private address. A public URL returning an HTTP redirect to a private service is another possible bypass. DNS can change between registration and delivery or between DNS validation and socket connection.

INT-001 enforces network destination policy **when creating/updating subscriptions and at delivery time**, and does not trust stored legacy URLs.

## Operational implementation

1. `Fintrox.Domain/Integrations/WebhookTargetPolicy.cs` normalizes strictly absolute HTTP(S) URLs, rejects credentials, fragments, single-label/local/internal/reserved names, disallowed literal IPv4/v6 addresses and alternative numeric representations, and tests IP address routability.
2. `WebhookSubscription.Create` and `Update` now use that policy. Rejected targets raise an argument exception before persistence.
3. `WebhookDeliveryWorker` validates the stored target again on each delivery and permanently fails already-stored unsafe endpoints before any network access.
4. The named HTTP client `Fintrox.Webhooks` uses `WebhookOutboundTransport.CreateHandler` in `Fintrox.Infrastructure/Integrations`, a dedicated `SocketsHttpHandler` configured with `AllowAutoRedirect=false`, `UseProxy=false`, `UseCookies=false`, and a connection timeout.
5. `SocketsHttpHandler.ConnectCallback` resolves the target via DNS **once per new connection**, rejects empty and mixed public/private address sets, and opens the socket directly to one of those validated IP addresses. TLS continues to validate against the original URL hostname. No second hostname resolution is done by the socket.
6. Blocked destinations are permanent delivery failures, even if an exception is wrapped by `HttpClient`. Other network failures continue to follow the worker's pre-existing retry policy.

Public HTTP(S) destinations on custom ports remain supported, provided the destination is publicly routable. HTTP redirects (3xx responses) are not followed, even to public destinations.

## Security test coverage

`tests/Fintrox.Domain.Tests/Integrations/WebhookTargetPolicyTests.cs` adds seven tests covering:
- canonical public HTTPS and public IPv4/IPv6 URLs;
- rejection of localhost, loopback, RFC1918, CGNAT, link-local, special-use/meta-data destinations, IPv4-mapped IPv6, 6to4, credentials/fragments, reserved service-discovery suffixes, alternate numeric forms and unsafe updates;
- IP filtering for public and special-purpose IPv4 and IPv6;
- unsafe or mixed DNS answers, including a public and a private address in the same response;
- public-only resolution and exact reuse of the validated result;
- rejection before the DNS resolver is even invoked;
- primary handler redirect/proxy/cookie settings and custom `ConnectCallback` registration.

CI uses deterministic faked DNS answers for these tests and **does not** rely on public DNS services.

## Explicit limitations and follow-up

- This is **destination SSRF protection**, not a completed Integrations module migration; the new 28 module Domain entities remain unmapped scaffolding. Existing integration persistence remains authoritative.
- An outbound network policy on hosting/infrastructure should provide another layer of protection; code cannot guarantee policy outside the Fintrox application or against arbitrary hosting/network misconfiguration.
- INT-002 is still pending: durable multi-worker webhook delivery claims, leases, retries and recovery are a separate concern.
- The HTTP webhook feature does not promise that a recipient processed a delivered event exactly once. Consumers should verify HMAC and process idempotently.
- The financial system of record remains [Fintrox](financial-system-of-record.md); financial-source actions must also satisfy integration authorization, tenant scoping and financial idempotency rules.
