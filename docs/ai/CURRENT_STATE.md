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

- 2026-10-14: ACCOUNTING-001F1 implemented in operational legacy Accounting: read-only source-to-GL integrity report for Sales, Purchases and Payments with current lifecycle, original journal, source FX conversion, exact reversal, missing/orphan, payment allocation checks; 11 migrated PostgreSQL HTTP tests added, not executed. Banking/AR-AP aging remain pending; no claim of full financial reconciliation or module EF migration. See docs/accounting-source-ledger-reconciliation.md.

- 2026-10-14: ACCOUNTING-001F2 CODED (CI NOT VERIFIED). Tenant-scoped AR/AP open-items report: each invoice/purchase, per-counterparty totals, payment allocation, original FX base carrying value, aging, historical/current control account GL reconciliation including manual differences; 11 migrated PostgreSQL HTTP regression methods authored, not executed. Banking statement matching and true historical as-of snapshot still pending. See docs/accounting-open-item-reconciliation.md.

- 2026-10-14: ACCOUNTING-001F3 coded: first operational Banking persistent tables/migration, tenant-scoped bank accounts with dedicated base-currency Asset GL account, immutable signed bank statement import/idempotent replay, exclusive Confirmed BankTransfer matching backed by actual Posted System journal, repeatable-read statement vs cumulative GL reconciliation and unmatched book/bank status. 9 PostgreSQL HTTP tests written, not run; CI unverified. Not FX, bank feed/ISO20022 parser or full company statutory close. See docs/accounting-bank-statement-reconciliation.md.

- 2026-10-14: ACCOUNTING-001F4A partial Banking automation coded in legacy API: strict 1-MiB/10k-row Fintrox-schema UTF-8 CSV import, existing immutable SHA256 replay; suggestions and exact auto-match only for unique confirmed base-currency bank transfers with matching system posted GL journal and exact date/sign/amount. 5 PostgreSQL HTTP + 4 parser unit regressions written, NOT RUN, CI/unittest unknown. Bank-fee GL, FX conversion, bank feeds and formal close evidence still NOT implemented. See docs/accounting-bank-csv-auto-match.md.

- 2026-10-14: ACCOUNTING-001F4B coded: immutable bank-fee-to-journal evidence migration, authorized explicit negative fee GL DR Expense / CR bank Asset system posting under bank/line and period locks, replay-safe reference, exclusive from payment match, validated posted fee count in bank reconciliation. Six migrated PostgreSQL HTTP tests written, not run. No automatic fee VAT, no controlled fee reversal, no FX or bank close signoff. See docs/accounting-bank-fee-posting.md.

- 2026-10-14: ACCOUNTING-001F4C1 realized foreign-bank FX settlement CODED, NOT COMPILED/CI VERIFIED. Active configured foreign bank account currencies, immutable native bank line + approved dated FX rate snapshot, explicitly selected AR/AP clearing carrying base and FX gain/loss control accounts, balanced bank/clearing/realized FX GL posting with fiscal locks and immutable journal link. Foreign bank report explicitly refuses native vs base balance subtraction. Six migrated PostgreSQL HTTP tests authored, NOT EXECUTED. Unrealized revaluation / native balance reconciliation (F4C2/C3), bank FX fees and sign-off remain pending. See docs/accounting-foreign-bank-settlements.md.

- 2026-10-14: ACCOUNTING-001F4C2 CODED, NOT BUILD/CI/POSTGRES TEST VERIFIED. Operational legacy Banking supports guarded, immutable, replay-safe period-end *unrealized* foreign bank FX revaluation with tenant selected exact cutoff FX rate, native statement history, posted source GL evidence and expected cumulative bank carrying value; balances journal DR bank/CR FX gain or DR FX loss/CR bank; zero-delta keeps immutable audit evidence only. Nine migrated PostgreSQL HTTP cases written, not executed. No automated foreign overdraft, nonzero opening import migration, formal exchange-rate signoff, native/base reporting certification or fee correction. See docs/accounting-foreign-bank-revaluation.md.

- 2026-10-14: ACCOUNTING-001F4C3A internal bank-native/base bridging and maker-checker approved review CODED (CI/Postgres tests NOT RUN). New immutable tenant preparation and separate approval, fingerprint tied to original statement/cutoff revaluation, GL movement validation and source-ledger audit; independent reviewer user ID and organizations.manage permission. 5 PostgreSQL HTTP regressions written, not executed. Not statutory or whole-enterprise close; controlled correction/reversal F4C3B outstanding. See docs/accounting-foreign-bank-close-review.md.

- 2026-10-14: ACCOUNTING-001F4C3B1 code in legacy Banking: append-only bank fee / not-yet-valued realized FX correction evidence, guarded POST .../correct-posting, serialized source and calendar locking, actual full inverse GL System journal, original marked Reversed, replay idempotency, changed retry and cross-tenant restrictions, original bank source kept immutable and flagged unmatched. Protected generic GL route cannot reverse its compensation journal. 8 real migrated PostgreSQL HTTP cases added but NOT RUN; .NET CI unverified. Approved FX revaluation/close versioned corrections pending as F4C3B2. See docs/accounting-bank-posting-corrections.md.

- 2026-10-14: ACCOUNTING-001F4C3B2 CODED in active legacy Banking, not compile/CI verified: additive immutable PostgreSQL FX valuation amendments v2+, exact vN originals and reversed/compensating/replacement System GL journals, exclusive fiscal period posting lock, strict no-downstream-history precondition, native/base GL evidence reread, new per-version maker/checker approval (original v1 remains immutable but is invalid for v2+), versioned source SHA and original rate snapshots, 9 new real migrated PostgreSQL HTTP tests written, not run. See docs/accounting-versioned-fx-bank-corrections.md.

- 2026-10-15: Verified cumulative ACCOUNTING CI #503 (https://github.com/viktor132607/Fintrox.Server/actions/runs/38654907133) at code SHA e28a16a11c695cdec191448f4d353f9f7435c089: Release dotnet build --warnaserror, **3,031 of 3,031 tests passed**, EF model snapshot clean, migrations 20261014000100–20261014000700 successfully applied/listed on PostgreSQL 18.6, Docker image built/published. Old CI-unverified statements are superseded for this code SHA. Enterprise-wide statutory financial close and new target module service integration still outstanding.
\n2026-10-15 — ACCOUNTING-001G1 CODED (CI PENDING): legacy Accounting added immutable tenant-scoped monthly recurring journal templates (fixed 1–28 day, optional final date, 2–100 balanced debit/credit lines), authorized GET/POST templates, secured one-per-date manual generate route with template FOR UPDATE lock and normal GL fiscal period locks, normalized append-only PostgreSQL evidence, current account revalidation, System Draft+lines inserted atomically, separate authorized GL post and protection against manual mutation of the generated draft. Five actual migrated-PostgreSQL HTTP regression methods authored, NOT VERIFIED YET. See docs/accounting-recurring-journals.md; ACCOUNTING-001G2 scheduler/revisions and 001H accruals pending; no automatic posting.\n\n2026-10-15 — ACCOUNTING-001G1 CI VERIFIED on source SHA a7544869081888e7c090c9abba32919cb3316e4b: GitHub CI/CD #522 (38658049296): Release dotnet build --warnaserror SUCCESS, 3036/3036 tests passed with 0 failures and 0 skips, EF model snapshot clean, migration 20261015000100_RecurringJournalTemplates applied and listed on PostgreSQL 18.6, Docker Build & Publish SUCCESS. Added immutable monthly recurring journal templates and tenant-scoped one-per-schedule-date System Draft generation, with explicit separate authorized GL posting. No automatic scheduling. ACCOUNTING-001G2 (template revisions, scheduling/monitoring) and 001H accruals/deferrals pending. Whole ACCOUNTING-001 remains IN PROGRESS.\n

2026-10-15 — ACCOUNTING-001G2A COMPLETE, CI VERIFIED [#526](https://github.com/viktor132607/Fintrox.Server/actions/runs/38772495791)
at source `f5d231d6ab9ee08a44205ff70731fa27a2b85a82`: read-only tenant-scoped recurring schedule projection
(`GET /api/v1/accounting/recurring-journals/{id}/schedule`) returns
Scheduled/Due/Overdue and posted GL evidence Draft/Posted/Reversed with
as-of semantics, max 366 days, no auto-posting. 3 new migrated-PostgreSQL
HTTP tests; **3039/3039 passed**, Release build 0 warnings/errors,
EF snapshot and PostgreSQL migrations clean, Docker publication passed.
ACCOUNTING-001G2B versioned amendments/orchestration next; enterprise
statutory close and full Accounting module cutover still pending.


2026-10-16 — ACCOUNTING-001G2B CODED, CI PENDING: migration
`20261016000100_RecurringJournalTemplateRevisions` adds immutable
tenant-composite-FK lineage (root, predecessor, successor, version,
effective-from first of month). `POST /api/v1/accounting/recurring-journals/{id}/revise`
atomically creates a replacement template and numbered edge under
predecessor FOR UPDATE lock; rejects parent already superseded, a
cutoff in/preceding parent's first month, invalid schedules/accounts/
unbalanced lines, or prior generated journal on/after cutoff.
`GET .../{id}/versions` reports the chain for any member. Existing
`generate` and `schedule` enforce effective cutoff without mutating
historic evidence or enabling auto-posting. Five real authenticated
PostgreSQL HTTP regressions authored, not yet executed. Next validate
CI, then ACCOUNTING-001G2C opt-in durable draft-generation scheduling.
Full ACCOUNTING-001 and statutory close remain IN PROGRESS.


2026-10-16 — ACCOUNTING-001G2B **COMPLETE — CI VERIFIED**:
[GitHub CI/CD #531](https://github.com/viktor132607/Fintrox.Server/actions/runs/38808420273) at application SHA
`3444b68430cfca6509e39dc090f70c392f46ce8d`: Release build warnings-as-errors **0 warnings/errors**,
**3044/3044 tests passed**, 0 skipped/failed, EF model snapshot clean,
migration `20261016000100_RecurringJournalTemplateRevisions` applied
and listed in real PostgreSQL, Docker Build & Publish successful.
This supersedes prior G2B CODED/CI PENDING statements. The immutable
revision chain, cutover lock, historical generation replay, tenant
isolation and concurrent single-successor tests are validated.
Next `ACCOUNTING-001G2C`: opt-in leased generation execution;
`ACCOUNTING-001H`: accruals and deferrals. Neither is implemented.
Entire `ACCOUNTING-001` remains IN PROGRESS; this is not statutory close.
