# Idempotency and external references

Phase 20 makes machine-to-machine writes replay-safe and preserves source-system identifiers for traceability.

## Idempotency key

Every mutating request authenticated with an integration client must include:

```text
X-Source-System: shop-api
X-External-Id: order-12345
X-Event-Type: order.created
```

The durable uniqueness key is:

```text
(OrganizationId, SourceSystem, ExternalId, EventType)
```

At the database level Fintrox enforces a unique constraint on those four values.

`SourceSystem` and `EventType` are normalized to lowercase. `ExternalId` is trimmed but remains case-sensitive.

## Request fingerprint

Fintrox computes a SHA-256 fingerprint over:

- HTTP method;
- path and query string;
- raw request body.

If the same external event key is reused with a different fingerprint, Fintrox returns `409 Conflict`. This prevents an upstream system from silently changing the payload behind an already-used external id.

## Atomic processing

The integration request ledger and the business operation execute inside one PostgreSQL transaction.

Existing internal transaction runners detect the outer idempotency transaction and reuse it instead of opening a nested transaction.

For responses below HTTP 500:

- the business operation is committed;
- the external event key is marked completed;
- status code, content type, response body and resource reference are stored.

For HTTP 5xx responses or exceptions, the entire transaction is rolled back. The same external event may then be retried safely.

A PostgreSQL transaction advisory lock serializes concurrent requests with the same external event key before the unique-key check.

## Replay behavior

If the same key and same request fingerprint arrive again after completion:

- Fintrox does not execute the business operation again;
- the stored HTTP response is returned;
- response header `Idempotency-Replayed: true` is added.

If the same key is already associated with different request content or route, Fintrox returns `409 Conflict`.

## Traceability

Each completed request stores:

- organization id;
- integration client id;
- source system;
- external id;
- event type;
- request method/path;
- request SHA-256;
- HTTP response status;
- resource reference;
- created/completed timestamps.

Human administrators with `integrations.manage` can inspect the ledger:

```text
GET /api/v1/integrations/requests
GET /api/v1/integrations/requests/{requestId}
```

List filters:

- `sourceSystem`
- `externalId`
- `eventType`

The response body is retained internally to support exact replay, but it is not exposed by the traceability API.

## Scope

The middleware applies only to authenticated integration-client requests and only to mutating HTTP methods.

Human user requests are unchanged. Safe methods such as GET and HEAD do not require external-event headers.
