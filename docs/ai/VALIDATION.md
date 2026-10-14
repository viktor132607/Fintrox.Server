# Validation history — 2026-10-14

## ACCOUNTING-001C / transactional accounting close controls
- **IMPLEMENTED, CI/.NET/real PostgreSQL execution NOT YET VERIFIED**. Active legacy `FiscalCalendarService` now closes/reopens periods/years within `ITransactionRunner` transactions using year-then-period exclusive PostgreSQL row locks; rejects unresolved Draft journals and period-misdated journals; requires all child periods closed before year hard close, no open periods before year soft close.
- Journal draft create/update/line mutations now acquire posting/calendar locks, recheck period/year after waiting and reject edits in closed/soft-closed periods. Six fiscal status-transition endpoints additionally require `accounting.post` alongside `accounting.write`.
- 6 new real HTTP + migrated PostgreSQL tests added: draft close gates, soft close/reopen, final year close and child reopen, year soft-close state, cross-tenant rejection, and blocking lock contention. These test cases have **NOT YET BEEN RUN**.
- This does **not** perform statutory year-end entries, retained-earnings transfer, accruals/deferrals, subledger reconciliations or signed close checklist. See [period close controls](../accounting-period-close-controls.md).

## ACCOUNTING-001B / first-year opening ledger balances
- IMPLEMENTED on `main`, **CI/build/real PostgreSQL execution unverified**. Added `ImportOpeningBalancesRequest`, `JournalService.ImportOpeningBalancesAsync`, repository first-year lock and existing-posted check, dedicated authorized HTTP controller; existing `System` source reference uniqueness is used, **no new migration**.
- Added 8 real migrated PostgreSQL HTTP tests to `FinancialHttpPostgresRegressionTests`: balanced import/idempotent replay, unbalanced/foreign account, already posted year, changed retry, tenant auth, two concurrent identical imports, P&L account restriction and closed calendar rejection.
- Multi-book, multi-currency, AP/AR subledger balances, bank/inventory matching and module EF migration still pending. See [opening balances](../accounting-opening-balances.md).

## ACCOUNTING-001A / concurrent posting and fiscal close safety
- IMPLEMENTED on `main`, **CI/build/test verification pending**; changes in `IJournalRepository`, `JournalRepository`, `JournalService`, `AutoPostingEngine` and `FinancialHttpPostgresRegressionTests`.
- Four migrated PostgreSQL HTTP tests added: simultaneous post one winner; simultaneous reversal one winner; period close vs posting; fiscal-year soft-close vs posting. Real runs and results must be recorded after CI, not inferred from test code.
- Shared PostgreSQL fiscal year / period row locks and exclusive journal lock protect status rechecks and numbering. See [posting concurrency](../accounting-posting-concurrency.md).

## API-001 / exact CORS policy and auth endpoint rate limits
- IMPLEMENTED, **CI verification pending**. Source files: `src/Fintrox.Api/Security/ApiRequestSecurity.cs`, `src/Fintrox.Api/Program.cs`, auth and integration-token controllers, default `appsettings.json` deny list.
- Six WebApplicationFactory HTTP regression tests added under `tests/Fintrox.Domain.Tests/Api/ApiRequestSecurityHttpTests.cs`; intended coverage: configured/unconfigured/unknown CORS origins, preflight, 429 login/register/integration-token quotas, isolation of unrelated endpoints, and forwarded-IP header spoofing resistance.
- Server rules: explicit `Cors:AllowedOrigins`, no wildcard/credentials, per-observed-IP 5 registration/hour, 10 login/minute, 30 refresh/minute, 30 revoke/minute, 20 integration token/minute. See [API request security](../api-request-security.md).
- **No CI, .NET build or HTTP test execution result claimed yet**. No local dotnet SDK available. No migration or schema changes.

## INT-002 / webhook atomic claims and expired lease recovery
- VERIFIED code commit `ec7261f01ad99dd8184d985d5b96c0072438f730`, GitHub Actions **38475619708**, Build/Test/Migrations job: Release build **0 warnings, 0 errors**, **2,906/2,906 tests passed**, zero failed or skipped, EF model snapshot and PostgreSQL migration application/listing passed.
- Added 4 PostgreSQL integration tests on disposable **migrated PostgreSQL** databases: competing worker claims only one winner; expired lease takeover fences stale results; retry is not available early; expired sixth attempt is terminal and not counted as a seventh HTTP attempt.
- Existing `integration.webhook_deliveries` columns implement the lease: `NextAttemptAtUtc` marks expiry, `AttemptCount` generation, `LastAttemptAtUtc` last claim. Atomic ExecuteUpdateAsync claims across worker instances; conditional finalization prevents a stale worker from overwriting another. Terminal failure and failure evidence share an EF transaction.
- No new migrations or schema changes. See [webhook-lease-recovery](../webhook-lease-recovery.md).
- Important limitation: at-least-once semantics; lost responses and expired in-flight requests can cause duplicate physical HTTP sends. Downstream consumers must process stable delivery/event IDs idempotently.



## TEST-001 / full-stack accounting PostgreSQL and HTTP
- VERIFIED source commit `1a03dc25bfaf530dc7d6f880a71116c77410f555`, GitHub Actions **38437997153**, successful Release build **0 errors / 0 warnings**, **2,902 tests passed** (0 failed/skipped), EF model snapshot and real PostgreSQL migration application/listing passed.
- 7 integration tests added under `tests/Fintrox.Domain.Tests/Accounting/FinancialHttpPostgresRegressionTests.cs`. The actual ASP.NET Core API and JWT/tenant authorization execute against one **migrated disposable PostgreSQL database per test** (not an in-memory DB), with financial state asserted in PostgreSQL afterward.
- Validated: debit/credit balance and numbering, rollback of unbalanced post, reversal swaps and double-reversal denial, period closure, cross-tenant read/post/reversal denial, cross-tenant account reference denial, 401/403, database trigger that forbids deleting posted lines.
- Found and fixed actual ASP.NET Core contract bug: 44 positional record request DTOs had validation attributes with `[property:]` instead of `[param:]`, previously causing HTTP 500 before controllers ran.
- Earlier CI iterations uncovered test harness token-signing mismatch and API/test database mismatch; both were fixed. No production schema/data changed.
- Details: [financial HTTP/PostgreSQL regression coverage](../financial-http-postgres-regression.md).



## ORG-001 / Organizations last-owner concurrency protection
- VERIFIED in code commit `8ec105e1cd1727ef38a0c9a081728da8f30d26d9`, GitHub CI **38428892598**, Build/Test/Migrations job: Release build 0 warnings, 0 errors; **2,895 of 2,895 tests passed**, 0 skipped/failed; EF snapshot and PostgreSQL migration application/listing checks passed.
- All mutating membership operations take a PostgreSQL `FOR UPDATE` lock on the organization's `core.organizations` row via the existing transaction runner, then recheck manager and target before owner-count/persistence changes.
- Added 4 real PostgreSQL tests: simultaneous owner self-deactivations, owner demotions, duplicate member additions and lock waits/release-on-commit. These tests require `FINTROX_TEST_POSTGRES` and use isolated disposable databases.
- First CI attempt (38428124190) found the prior access-test repository fake lacked the new locking contract; second (38428507039) found manually initiated transactions in one test incompatible with enabled EF retries; the final run passed after correcting both. No database schema changes made.
- Detailed behavior, remaining boundaries: [organization-owner-concurrency](../organization-owner-concurrency.md).



## INT-001 / Integrations webhook SSRF
- VERIFIED in Server code commit `cb0c54384e57608e1937f803d4c99c0b30b1a1ba`, GitHub Actions run **38399255676**, Build/Test/Migrations job: Release build 0 warnings, 0 errors; **2,891 of 2,891** tests passed (0 skipped/failed); EF model snapshot and PostgreSQL migration application/listing passed.
- 7 new webhook URL/DNS/transport tests added. Validation includes reserved/private/mapped/special-use addresses, local DNS, mixed public/private answers, public-only pinned answers, no redirects/proxies and reject-before-DNS.
- Initial CI runs 38398418239/38398639909 found C# overload/CA1865 conflicts; 38398866280 found that `.svc` service-discovery names were allowed by initial structural URL validation. All issues were fixed and the final run passed.
- No external public network integration test was performed; the public-target behavior is verified deterministically via policy and injectable DNS resolver tests. Docker publication is a separate CI job.
- No EF model or database schema change was required. Details: [webhook security](../webhook-security.md).


## ORG-002 / Identity–Organizations foundation
- VERIFIED: Server commit 829cddeadb49202ecc17fa88d1876eb0596f138d, GitHub CI 38329276594, Build/Test/Migrations: Release build 0 warnings/errors, 2,884 tests passing (0 failed/skipped), EF snapshot passed, PostgreSQL migrations applied/listed successfully.
- Changes: live tenant-activity check in OrganizationAccessService, explicit owner-only reactivation permission in OrganizationService, and 7 regression tests.
- The original 2,877-test milestone below predates ORG-002; 2,884-test CI has now passed.


## ENTITIES-EXPANSION (2026-10-13)
- VERIFIED for code commit 90d601e935c50f8d0874387d8db753962d1e821a in GitHub Actions run 38324236400, Build/Test/Migrations job 115059092795: Release build succeeded with 0 warnings, 0 errors; 2,877 tests passed, 0 failed, 0 skipped; EF model snapshot verification passed; PostgreSQL migrations applied and listed successfully. Docker publish job is separate.

- CI run 38324015336 reached the Release build and identified five CA1711 type-naming analyzer errors (entity names ending Attribute / Exception). Renamed those model classes and updated all affected inventory paths and module documentation. New CI pending.
- First expanded-head CI (38323676459) identified reserved C# keyword used as constructor parameter in Capabilities/CapabilityConstraint.Operator; fixed with escaped @operator in the next commit. Other generated identifiers were scanned against reserved C# keywords. New CI/build result pending.
- All 36 modules expanded by 756 additional Domain entity classes: 936 total inventoried module model types; the 32 legacy core persistence types remain unchanged.
- Recursive GitHub tree verified 936 distinct entity model source paths, all matched by the machine inventory; tree result was not truncated.
- Verified 36 sequential per-module progress records and 36 module-status rows.
- Expanded the inventory checker to verify field-property names/types and fixed dynamic reflection test samples for nationality and supply-country codes.
- No local .NET SDK: build, tests and PostgreSQL migration verification have now passed in the above GitHub Actions job. Previous 604 local passing tests below predate the new models.
- Domain definitions only. EF mappings, migrations, authorization, application services, APIs and UI remain pending.


## ENTITIES-001 current validation
- Static architecture and full 180-entity/36-module inventory checks PASSED.
- Local Release build of the affected test/dependency graph with warnings-as-errors PASSED. 604 tests passed; 5 PostgreSQL tests explicitly skipped locally (no server). CI full solution and database verification pending.
- New regression coverage: 540 entity identity/scope/date cases plus 18 business-boundary tests. No module persistence or API integration claimed.

## Previous verified state
Code commit: 422cc38dd2a20200ca3a5e1ee91a8c6c78d876dd (AUTH-002).
CI run 38012762635, job 114096261806: restore, Release build with warnings as errors (0 warnings/errors), 51/51 tests (0 skipped), EF snapshot and PostgreSQL migration application/listing PASSED.
Five PostgreSQL tests cover simultaneous refresh, revoke winning during refresh read, revocation after rotation, failed replacement-save rollback/same-context retry, and session ownership/revocation.
Local affected-project Release build and 46 unit tests passed; five relational tests explicitly skipped locally (PostgreSQL unavailable), then all five passed in CI.
Architecture check passes. No new migration/project or production verification.
Previous AUTH-001 code 79ee090 also passed full build, 46/46 tests and migrations (CI 37900969688).
Historical scaffold-only NOT RUN entries below are superseded by CI evidence.

## Initial scaffold checks (historical)

- PASS: 36 module directories, 180 module projects, 5 core projects, 1 test project.
- PASS: 186 unique .slnx paths; every project exists; original 121 retained.
- PASS: every original solution folder/registration preserved unchanged.
- PASS: all original project XML unchanged except 26 additive API host references.
- PASS: 13 modules each contain five projects, requested Application features, README and tracked placeholder directories.
- PASS: dependency graph, missing targets, cycles, layer boundaries, catalog/capability coverage and host composition checked by scripts/check_architecture.py.
- PASS: negative verification rejects duplicate solution registrations and forbidden/cyclic project references.
- PASS: no new .cs business files, services, entities, DTOs, endpoints or migrations.
- NOT RUN: dotnet restore Fintrox.Server.slnx — dotnet executable not installed.
- NOT RUN: dotnet build Fintrox.Server.slnx --no-restore — same SDK blocker.
- NOT RUN: dotnet test Fintrox.Server.slnx --no-restore — same SDK blocker.
- NOT RUN: Visual Studio loading, database migration execution, deployment, production verification.

Commands for an SDK-capable environment:
```bash
python3 scripts/check_architecture.py
dotnet restore Fintrox.Server.slnx
dotnet build Fintrox.Server.slnx --no-restore
dotnet test Fintrox.Server.slnx --no-restore
```
SDK requirement: global.json requests 10.0.401 with latestFeature roll-forward.
Post-publication verification and actual commit SHA are recorded in SESSION_HANDOFF.md.

## Published commit verification
Architecture commit: c3f0ebfb1f2f4dc547cbaa1286bee61ae21de395 (main updated with expected-head check).
All 362 written files verified against GitHub Git blob hashes; all untouched baseline files retain their original hashes.
186 .csproj files confirmed in the published tree. No runtime verification performed.
