# INT-002 — Durable multi-worker webhook delivery leases

**Implementation commit:** `3920ceb8975959023590093743602daf8fe59536` (code) and `f1004bfafb5ab874792d4f61387ff9026a3601fd` (real PostgreSQL tests), with timestamp-boundary test correction `ec7261f01ad99dd8184d985d5b96c0072438f730`.

**Status:** COMPLETE — CI VERIFIED. Source commit `ec7261f01ad99dd8184d985d5b96c0072438f730`, [GitHub Actions 38475619708](https://github.com/viktor132607/Fintrox.Server/actions/runs/38475619708): .NET 10 Release build 0 warnings/errors, **2,906/2,906** tests passed (0 failed/skipped), EF model snapshot, migration application and migration list checks passed on real PostgreSQL. The Docker image publish job is separate.

## Problem

The original worker queried pending deliveries, then loaded and saved a tracked `WebhookDelivery` from each worker independently. Multiple processes could send the **same delivery** concurrently; after a process crash, a delivery could be retried without ownership guarantees, and a stale worker could overwrite a later delivery outcome.

## Atomic claim and lease (existing PostgreSQL schema)

The operational repository now provides `ClaimNextDueDeliveryAsync`:
- Query up to 30 due `Pending` candidates ordered by retry time and creation date.
- Conditionally update a single candidate **in PostgreSQL** only while it is still Pending and due. A successful update sets `LastAttemptAtUtc`, increments `AttemptCount` (unless already at the limit), advances `NextAttemptAtUtc` to **now + two minutes**, and updates the audit timestamp. `ExecuteUpdateAsync` makes the claim atomic across server instances.
- A worker processes **one claimed delivery at a time**; a batch may process up to 20 claimed deliveries but does not reserve 20 while waiting for network traffic.
- `WebhookDeliveryClaim` carries the detached delivery snapshot and an `IsExhausted` flag for recovery beyond the configured 6 attempts.

The existing columns `NextAttemptAtUtc`, `AttemptCount`, and `LastAttemptAtUtc` are reused; `Pending` with a future next-attempt timestamp represents an active lease. **No migration or new PostgreSQL columns** were introduced.

## Crash recovery and stale-worker fencing

- A crashed worker stops updating its claim. After its two-minute lease expires, another process can atomically reclaim it and retry.
- Every result transition (Delivered/Failed/Pending retry) is guarded by a conditional SQL update against the **claimed attempt generation**, **original lease deadline**, **last-attempt timestamp**, organization and delivery identifier, and `Pending` status. A stale worker receives `false` and cannot overwrite a newer claim.
- On success, the delivery becomes `Delivered` only if the worker still owns the claim.
- On retryable failure, an attempted delivery becomes Pending with the configured retry delay (1m, 5m, 15m, 1h, 6h), but only if the claim is still valid.
- On terminal failure, the terminal status and the associated `IntegrationFailure` record are written within a **single EF transaction**, avoiding a state where the terminal mark has committed but failure evidence was lost.
- If the sixth delivery lease expires without a result, its recovery claim is `IsExhausted=true`, keeps the counter at 6, and **records terminal failure without a seventh HTTP call**.

## Delivery guarantee

These changes guarantee **at most one current database lease owner**, fenced completion, and recovery of expired leases. They do **not** promise exactly-once HTTP processing. A recipient may have processed a request whose response was lost when the sender crashed; a later attempt can redeliver the event. Consumers must verify the existing HMAC signature, deduplicate by `X-Fintrox-Delivery-Id` / event ID, and process using idempotent business operations. The event and delivery IDs remain stable across attempts.

The two-minute lease exceeds the named HTTP client's 30-second timeout under ordinary operating conditions, but excessive network delays or application stalls can still allow overlapping physical HTTP requests. The database prevents stale finalization, **not** every possible simultaneous physical transmission.

## PostgreSQL regression coverage

`tests/Fintrox.Domain.Tests/Integrations/WebhookDeliveryLeasePostgresTests.cs` tests:
1. Two independent concurrent repository workers can claim the same due delivery only once.
2. Expired lease is reclaimed; the previous worker's late Delivered update is rejected; the new worker can finalize.
3. Retry scheduling releases a claim only at the new retry due time.
4. Expired final attempt is recovered as terminal without a seventh claimed increment/send.

Each test creates a dedicated disposable PostgreSQL database, uses the existing EF migrations and repository implementations, and cleans up its database. Connection env var: `FINTROX_TEST_POSTGRES`.

## Operational boundaries

- Feature implemented in current **legacy** Integrations runtime. The new Integrations module's entity model scaffolds are not yet fully wired to application/EF/API.
- Workers must use the repository's atomic claim + CAS transition; direct DB updates bypassing these methods are not protected.
- This work is separate from INT-001 (webhook destination SSRF protection) and financial integration idempotency.
- No changes to frontend or externally visible webhook subscription API contracts.
