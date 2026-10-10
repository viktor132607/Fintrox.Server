# Task queue

| Task ID | Title | Module | Priority | Dependencies | Status | Acceptance Criteria |
|---|---|---|---|---|---|---|
| ARCH-001 | 13 new module scaffolds | Platform | P0 | Baseline | COMPLETE | 36 modules, 186 projects, architecture passes |
| VERIFY-001 | Verify baseline restore/build/tests | Platform | P0 | ARCH-001 | COMPLETE | CI 37860035722: restore/build/33 tests/migrations pass |
| AUDIT-001 | Audit Server and Client | All | P0 | VERIFY-001 | COMPLETE | AUDIT.md covers all modules, layers, frontend and concrete gaps |
| AUTH-001 | Revalidate issued bearer tokens against current state | Identity/Integrations | P0 | AUDIT-001 | COMPLETE | Commit 79ee090; CI build, 46 tests and PostgreSQL migrations pass |
| AUTH-002 | Atomic single-use refresh-token rotation | Identity | P0 | AUTH-001 | COMPLETE | 422cc38: one winner, revoke ordering, rollback; CI 51/51 tests and migrations pass |
| INT-001 | Webhook SSRF prevention | Integrations | P0 | AUDIT-001 | PENDING | Private/loopback/metadata targets and redirect/DNS bypass rejected; safe public delivery tested |
| ORG-001 | Concurrent last-owner protection | Organizations | P1 | AUDIT-001 | PENDING | Simultaneous removal/demotion cannot leave no active owner |
| ORG-002 | Inactive-tenant business access policy | Organizations | P1 | AUDIT-001 | PENDING | Deny inactive tenant business operations while preserving authorized reactivation |
| TEST-001 | Financial/tenant/HTTP regression coverage | Accounting/Platform | P1 | AUDIT-001 | PENDING | Real PostgreSQL posting/reversal, balance/period and cross-tenant isolation tests |
| INT-002 | Durable multi-worker webhook claim/recovery | Integrations | P1 | INT-001 | PENDING | Atomic claims, expired-lease recovery, retry/duplicate-delivery tests |
| API-001 | Configured CORS and endpoint throttling | API | P1 | Authentication hardening | PENDING | Explicit origins, preflight and auth endpoint rate tests |
| ACCOUNTING-001 | Complete audited accounting gaps | Accounting/Tax/Currencies | P1 | Foundation/tests | PENDING | See AUDIT.md and MASTER_ROADMAP phases 2–3 |
| FE-001 | Align 36 frontend shells/manifests | Client | P2 | ARCH-001 | PENDING | Module parity and TS architecture/typecheck/build |
| ERP-001 | Operational/new module business implementation | All | P2 | Accounting foundation | PENDING | Incremental use cases with auth/persistence/API/tests |
| ENTERPRISE-001 | Enterprise/hardening | All | P3 | Core ERP | PENDING | MASTER_ROADMAP phases 9–10 |

The user authorized continuation after the model switch; the previous stop-after-scaffold instruction is fulfilled.
