# Task queue

| Task ID | Title | Module | Priority | Dependencies | Status | Acceptance Criteria |
|---|---|---|---|---|---|---|
| ENTITIES-001 | Module entity foundation | All 36 | P0 user-directed | ARCH-001 | IMPLEMENTED — CI PENDING | 180 named types, all module boundaries preserved, invariant tests, per-module progress |
| ENTITIES-EXPANSION | Deepen granular Domain models in all 36 modules | All 36 | P0 user-directed | ENTITIES-001 | COMPLETE — VERIFIED | 756 added; 936 inventoried; per-module commits/docs; Release build, 2877 tests, EF/PostgreSQL verified in CI 38324236400 |
| FIN-SOR-001 | Central enterprise financial system of record contract | All financial modules | P0 user-directed | Architecture | SPECIFIED — DELIVERY IN PHASES | Trace all money events to a single Fintrox ledger with authorized, idempotent source-to-posting process; see docs/financial-system-of-record.md |
| ENTITIES-002 | Integrate entity persistence and use cases | Incremental modules | P1 | ENTITIES-001 | PENDING | EF constraints, authorization, contracts, lifecycle behavior, PostgreSQL and HTTP tests per capability |
| ARCH-001 | 13 new module scaffolds | Platform | P0 | Baseline | COMPLETE | 36 modules, 186 projects, architecture passes |
| VERIFY-001 | Verify baseline restore/build/tests | Platform | P0 | ARCH-001 | COMPLETE | CI 37860035722: restore/build/33 tests/migrations pass |
| AUDIT-001 | Audit Server and Client | All | P0 | VERIFY-001 | COMPLETE | AUDIT.md covers all modules, layers, frontend and concrete gaps |
| AUTH-001 | Revalidate issued bearer tokens against current state | Identity/Integrations | P0 | AUDIT-001 | COMPLETE | Commit 79ee090; CI build, 46 tests and PostgreSQL migrations pass |
| AUTH-002 | Atomic single-use refresh-token rotation | Identity | P0 | AUTH-001 | COMPLETE | 422cc38: one winner, revoke ordering, rollback; CI 51/51 tests and migrations pass |
| INT-001 | Webhook SSRF prevention | Integrations | P0 | AUDIT-001 | COMPLETE — CI VERIFIED | Domain URL policy, mixed-DNS rejection, connect-time IP pinning, no redirect/proxy, 7 security tests; 2,891 passed; CI 38399255676 |
| ORG-001 | Concurrent last-owner protection | Organizations | P1 | AUDIT-001 | COMPLETE — CI VERIFIED | Organization-row PostgreSQL FOR UPDATE lock, transactional membership changes and 4 PostgreSQL concurrency tests; 2,895 passing, CI 38428892598 |
| ORG-002 | Inactive-tenant business access policy | Identity/Organizations | P1 | AUDIT-001 | COMPLETE — CI VERIFIED | Active membership and organization required; deactivated tenant only owner reactivation; 7 regression tests; CI 38329276594 (2884 passing) |
| TEST-001 | Financial/tenant/HTTP regression coverage | Accounting/Platform | P1 | AUDIT-001 | COMPLETE — CI VERIFIED | 7 full-stack HTTP + migrated PostgreSQL scenarios: posting, reversal, balance, period, tenant, auth and DB immutability; 2902 tests passing; CI 38437997153 |
| INT-002 | Durable multi-worker webhook claim/recovery | Integrations | P1 | INT-001 | COMPLETE — CI VERIFIED | Atomic PostgreSQL lease/CAS outcomes, expiry recovery and max-attempt handling; 4 real PostgreSQL tests, CI 38475619708, 2,906 passed; docs/webhook-lease-recovery.md |
| API-001 | Configured CORS and endpoint throttling | API | P1 | Authentication hardening | IMPLEMENTED — CI PENDING | Explicit origins, preflight and auth endpoint rate tests |
| ACCOUNTING-001G1 | Monthly recurring journal templates and manual one-per-date draft generation | Accounting | P1 | ACCOUNTING-001F | COMPLETE — CI VERIFIED #522 (3036/3036; EF + POSTGRES + DOCKER) | Immutable monthly balanced tenant templates, locks, idempotent Draft generation, explicit authorized GL posting; 5 PostgreSQL HTTP tests; see docs/accounting-recurring-journals.md |
| ACCOUNTING-001 | Complete audited accounting gaps | Accounting/Tax/Currencies | P1 | Foundation/tests | IN PROGRESS — 001A-E CODED; 001F1 SOURCE→GL, 001F2 AR/AP & 001F3 BASE-CURRENCY BANKING & 001F4A CSV/AUTO-MATCH + 001F4B FEE GL + 001F4C1 REALIZED FX + 001F4C2 BANK REVALUATION + 001F4C3A NATIVE/BASE BRIDGE & MAKER-CHECKER + 001F4C3B1 SOURCE CORRECTIONS + 001F4C3B2 VERSIONED REVALUATION/REAPPROVAL CODED; CI VERIFIED #522 (3036/3036; EF, POSTGRES AND DOCKER); FORMAL MULTI-PERIOD CLOSE PENDING | See AUDIT.md and MASTER_ROADMAP phases 2–3 |
| FE-001 | Align 36 frontend shells/manifests | Client | P2 | ARCH-001 | PENDING | Module parity and TS architecture/typecheck/build |
| ERP-001 | Operational/new module business implementation | All | P2 | Accounting foundation | PENDING | Incremental use cases with auth/persistence/API/tests |
| ENTERPRISE-001 | Enterprise/hardening | All | P3 | Core ERP | PENDING | MASTER_ROADMAP phases 9–10 |

The user authorized continuation after the model switch; the previous stop-after-scaffold instruction is fulfilled.
