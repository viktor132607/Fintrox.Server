# TEST-001 — accounting PostgreSQL / HTTP regression coverage

**Status:** COMPLETE, verified 2026-10-13.

**Verified source code commit:** `1a03dc25bfaf530dc7d6f880a71116c77410f555`
**CI:** [38437997153](https://github.com/viktor132607/Fintrox.Server/actions/runs/38437997153), Build/Test/Migrations succeeded: **2,902/2,902 tests passed**, zero failed/skipped, .NET 10 Release build with **0 warnings/errors**, EF model snapshot and PostgreSQL migrations applied/listed successfully.

## What was actually tested

The tests live in [`tests/Fintrox.Domain.Tests/Accounting/FinancialHttpPostgresRegressionTests.cs`](../tests/Fintrox.Domain.Tests/Accounting/FinancialHttpPostgresRegressionTests.cs). They do **not** mock the accounting service, HTTP controller, JWT validation, or PostgreSQL engine.

A per-test fixture creates a unique disposable PostgreSQL database using the `FINTROX_TEST_POSTGRES` administrator connection, calls `Database.MigrateAsync()` (so the real journal immutability triggers/constraints are present), seeds two independent organizations/users, membership/permissions, accounting periods, chart-of-accounts entries and then starts the **actual Fintrox API** in-process using `WebApplicationFactory<Program>`. Its DI options bind the server's `FintroxDbContext` to the same isolated database and signed JWT bearer tokens are issued by the actual host service. The background webhook worker is excluded from this test host only.

### 7 new end-to-end scenarios

| Test | Assertions |
|---|---|
| Balanced posting | Create journal draft through API; post; compare persisted state and entries, 4-decimal debit/credit totals, posting number and sequence |
| Unbalanced posting rejection | Unbalanced draft returns 409; remains Draft; consumes no sequence |
| Reversal and duplication | API reversal creates a new posted entry with debits and credits swapped, original marked Reversed; second reversal rejected 409 |
| Fiscal period closure | Existing draft cannot be posted and posted entry cannot be reversed within a closed period; existing data/sequence unchanged |
| Tenant isolation | Tenant B cannot fetch, post or reverse Tenant A's journal; cross-tenant account cannot appear on a new journal line; spoofed organization header is forbidden |
| Authentication | Anonymous GET rejected 401; signed user request without a valid organization context rejected 403 |
| Database immutability | Attempt to delete posted journal lines using **direct SQL** is rejected by the migrated PostgreSQL trigger; data stays intact |

The tests assert HTTP status codes and **query the database afterward** for financial invariants; a successful HTTP response alone is insufficient. The anti-corruption boundary for source-to-ledger finance is still defined in [financial system of record](financial-system-of-record.md).

## Additional production defect discovered and fixed by these tests

ASP.NET Core rejects a positional record request DTO that carries DataAnnotations validation metadata with `[property: Required]` / `[property: MaxLength]`. It requires the validation metadata on the **primary-constructor parameter**. The existing DTOs caused HTTP 500 before controller execution. Fixed **44 affected request DTO files** across Accounting, Auth, Counterparties, Currencies, Integrations, Organizations, Payments, Purchases, Sales and Tax, replacing `[property: ...]` with `[param: ...]`. This keeps JSON field names, constructor signatures and request contract types stable; attribute targets now match ASP.NET Core's model validation.

## Verification iterations

The first runs exposed problems in the **test harness** (test-generated JWT did not match the host signing key; app host was using another database). The harness now signs with the live host token service and explicitly asserts the server uses the migrated disposable database. After fixing that, tests exposed the **real request validation metadata defect** noted above. The final CI run passes all 2,902 tests and migrations.

## Explicit limitations / next milestone

- This provides a strong regression baseline for the **operational legacy Accounting API**, **not** a claim that new module Accounting persistence/API has been migrated or that every accounting flow (multi-book, foreign currency, consolidated closing, fiscal law, bank settlement) has full tests.
- It does not prove all concurrency/approval cases or all financial integrations. Expand test cases per later Accounting, Tax, Banking, Payments and subledger milestones.
- No production finance data, live banking accounts or deployments were changed.
- **Next queued hardening item:** `INT-002` — durable multi-worker webhook claim/recovery. `API-001` (CORS/rate limiting) and `ACCOUNTING-001` remain pending.
