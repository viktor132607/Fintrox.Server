# Current state — 2026-10-11

- Architecture: 36 modules, 186 projects; 36 Domain assemblies now contain 180 new entities; other module layers remain scaffolds.
- VERIFY-001 COMPLETE: Server baseline 04e50ceb CI run 37860035722 passed restore, Release build (0 warnings/errors), 33 tests, EF snapshot validation and PostgreSQL migrations. Client run 37859970261 passed check/build.
- AUDIT-001 COMPLETE: see AUDIT.md for all 36 module statuses, actual legacy features, frontend gaps and prioritized risks.
- AUTH-001 COMPLETE at 79ee090e2e50ff64d3b3fb11d01e7fda31eb5b50: bearer OnTokenValidated checks current user activity or integration-client/organization activity and scope validity. CI run 37900969688 passed Release build (0 warnings/errors), 46/46 tests, EF model snapshot and PostgreSQL migrations.
- Existing financial logic, migrations, module scaffolds and frontend source unchanged.
- Local SDK 10.0.401 was restored under the scratch workspace after /tmp was cleared. Package restore, complete Release build (0 warnings/errors) and 46/46 tests passed locally and in CI at the exact code commit above.
- Client still has 23 TypeScript module shells. No operational business screens exist.
- Next P0 item: webhook SSRF policy (see TASK_QUEUE.md).

- AUTH-002 COMPLETE at 422cc38dd2a20200ca3a5e1ee91a8c6c78d876dd: transactional conditional refresh consumption and revocation. CI 38012762635 / job 114096261806 passed Release build (0 warnings/errors), 51/51 tests (0 skipped), EF snapshot and PostgreSQL migrations. Five real database regressions cover concurrency, revoke ordering, ownership and rollback. No migration or new project.

- ENTITIES-001: domain foundation implemented in all 36 modules; local affected-graph Release build and 604 tests passed; 5 PostgreSQL tests await CI. Per-module names and remaining work: MODULE_STATUS.md. New models are not yet runtime tables/APIs. Existing legacy entities, financial engines and migrations are unchanged.

- 2026-10-14: INT-002 verified (2,906 tests, CI 38475619708). API-001 implemented (explicit-origin CORS, IP-auth endpoint rate limits, 6 in-process HTTP tests) with build/CI verification pending. Next planned item ACCOUNTING-001. See TASK_QUEUE.md and VALIDATION.md; new module EF/API mappings are still pending.

- 2026-10-14: ACCOUNTING-001A posting/close race guard implemented and 4 real PostgreSQL HTTP regression tests added; CI pending. No accounting Domain module wiring/migrations. ACCOUNTING-001 overall in progress, next opening balances and period/closing workflows. See accounting-posting-concurrency.md.

- 2026-10-14: ACCOUNTING-001B opening balance import implemented in operational legacy GL; per-year idempotent balanced posted journal, fiscal year lock and 8 migrated PostgreSQL HTTP tests added. CI verification pending. Module EF/API migrations, AP/AR subledgers, FX and comprehensive period closing remain pending. See docs/accounting-opening-balances.md.

- 2026-10-14: ACCOUNTING-001C transactional period/year close and reopen gates implemented in legacy runtime (year/period row locks, reject draft/misdated journals, guarded draft edits, tighter permissions and guarded year/period edits); 10 migrated PostgreSQL HTTP cases added. Unverified CI. Statutory P&L close, reconciliation and new Accounting module persistence pending. See docs/accounting-period-close-controls.md.

- 2026-10-14: ACCOUNTING-001D implemented in active legacy Accounting: annual income/expense balances closed through an actual balanced and replay-safe posted journal, result transferred to equity, final period and year closed atomically. Added eight unverified real migrated PostgreSQL HTTP tests. Year status-only Close denies outstanding P&L. CI and production validation pending; full statutory close, reconciliation, new-module wiring pending. See docs/accounting-year-end-profit-loss.md.

- 2026-10-14: ACCOUNTING-001E implemented in active legacy GL: authorized controlled fiscal closing reversal with reason and exact source closing journal ID, original immutable reversal linkage, atomic final period/year reopening, idempotent retry, safe versioned reclose; read-only GL-only reconciliation of posted totals, annual P&L, drafts and fiscal calendar. 6 new disposable PostgreSQL HTTP tests in partial fixture files, not run. Statutory reconciliation and new Accounting module wiring remain pending. See docs/accounting-year-end-corrections.md.
