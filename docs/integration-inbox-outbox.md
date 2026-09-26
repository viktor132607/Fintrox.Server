# Integration Inbox / Outbox

Phase 21 adds the durable integration processing layer between external business events and Fintrox accounting logic.

## Boundary

Other applications send business events. They do not send journal entries or accounting instructions.

The public business-event HTTP endpoints are intentionally left to phase 22. Phase 21 implements the processing pipeline those endpoints call.

## Inbox

Each inbound business event is stored in `integration.integration_inbox` with:

- organization;
- source system;
- external id;
- event type;
- business operation;
- original JSON payload;
- processing status;
- retry count;
- last attempt;
- generated entity id/type;
- generated journal entry id when applicable;
- failure reason.

The unique event key is:

```text
(OrganizationId, SourceSystem, ExternalId, EventType)
```

Supported internal operations in phase 21:

- `sales`: creates and issues a sales invoice;
- `payment`: creates allocations and confirms a payment;
- `expense`: creates and receives an expense purchase document;
- `counterparty`: creates a counterparty.

Sales, payment and expense handlers reuse the existing auto-posting engine, so successful processing generates the accounting journal entry inside Fintrox.

## Processing transaction

The inbox row is stored first.

Processing then runs in a database transaction:

1. mark inbox item `Processing`;
2. deserialize and business-validate the payload;
3. call the existing Fintrox domain/application service;
4. create/issue/confirm/receive the business document;
5. allow the existing auto-posting engine to create the journal entry;
6. mark inbox item `Succeeded`;
7. create an outbox `IntegrationEvent`;
8. fan out matching webhook deliveries.

If the processing transaction fails, the generated business/accounting data is rolled back. The inbox item is marked `Failed` in a separate transaction and a durable `IntegrationFailure` stores the reason, payload, retry count and last attempt.

## Outbox

A successful operation creates an `integration.integration_events` record containing:

- event type;
- aggregate type/id;
- source inbox id;
- event payload;
- occurred/dispatched timestamps.

Current emitted event types:

- `sales.invoice.issued`;
- `payment.confirmed`;
- `purchase.expense.received`;
- `counterparty.created`.

Matching webhook subscriptions produce one durable delivery row per event/subscription pair.

## Webhook subscriptions

Management API:

```text
GET    /api/v1/integrations/webhooks
GET    /api/v1/integrations/webhooks/{id}
POST   /api/v1/integrations/webhooks
PUT    /api/v1/integrations/webhooks/{id}
POST   /api/v1/integrations/webhooks/{id}/rotate-secret
DELETE /api/v1/integrations/webhooks/{id}
POST   /api/v1/integrations/webhooks/{id}/activate
GET    /api/v1/integrations/webhooks/deliveries
```

All management endpoints require `integrations.manage`.

Subscriptions can select exact event types or `*`.

Signing secrets are returned only when a subscription is created or rotated. The stored secret is AES-GCM encrypted using `Integrations:WebhookEncryptionKey`. When that setting is absent, the configured JWT signing key is used as the encryption key material.

## Webhook delivery

The API hosts a delivery worker.

Each request includes:

- `X-Fintrox-Event-Id`;
- `X-Fintrox-Delivery-Id`;
- `X-Fintrox-Signature: sha256=<hex HMAC>`.

The HMAC is calculated over the exact UTF-8 JSON request body with the subscription signing secret.

Retry schedule:

```text
1 minute -> 5 minutes -> 15 minutes -> 1 hour -> 6 hours
```

After six failed attempts the delivery becomes `Failed` and creates a durable integration failure.

## Failures and retry

Admin endpoints:

```text
GET  /api/v1/integrations/inbox
GET  /api/v1/integrations/inbox/{id}
POST /api/v1/integrations/inbox/{id}/retry

GET  /api/v1/integrations/failures
GET  /api/v1/integrations/failures/{id}
POST /api/v1/integrations/failures/{id}/retry
```

Inbox retries re-run the original stored payload through the business handler.

Webhook failure retries requeue the failed delivery immediately.

Successful retries resolve the corresponding open failure.
