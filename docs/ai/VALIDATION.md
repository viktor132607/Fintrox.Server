# Validation history — 2026-10-15\n\n## ACCOUNTING-001G1 — monthly recurring GL draft generation\n- **CI VERIFIED:** GitHub Actions #522 (https://github.com/viktor132607/Fintrox.Server/actions/runs/38658049296) at code SHA a7544869081888e7c090c9abba32919cb3316e4b. Release build --warnaserror, **3036 passed/0 failed/0 skipped**, EF snapshot clean, PostgreSQL 18.6 migration `20261015000100_RecurringJournalTemplates` applied and listed; Docker Build & Publish successful. \n- IMPLEMENTED: normalized additive migration 20261015000100, immutable account templates, authorized GET/POST/generate API, one-per-month idempotent System Draft snapshots with separate posting and no duplicate period entries. 5 migrated HTTP/PostgreSQL tests authored and executed successfully in CI #522. See [design](../accounting-recurring-journals.md).

## 2026-10-15 — ACCOUNTING-001 cumulative verification: GREEN
- VERIFIED GitHub Actions CI/CD #503 (https://github.com/viktor132607/Fintrox.Server/actions/runs/38654907133), code SHA e28a16a11c695cdec191448f4d353f9f7435c089: successful Release dotnet build --warnaserror and actual migrated PostgreSQL tests: **3031 passed, 0 failed, 0 skipped**, net10.0.
- EF has-pending-model-changes: **No changes have been made to the model since the last migration**. EF migrations applied and listed on clean PostgreSQL 18.6, including all seven 20261014000100–20261014000700 bank source, fees, realized/unrealized FX, review, correction, and versioned correction migrations. Docker Build & Publish: SUCCESS.
- Concrete CI fixes since failed #484: implemented missing opening balance interface method; renamed reserved interface parameters and repaired LINQ; corrected analyzer globalization/static warnings and shadowed local; fixed test nullable and fiscal tuple errors; changed PostBankFeeRequest record validation to constructor parameters (HTTP 500 root cause); inserted test payment allocations while Draft before confirmation; supplied CORS origin early; removed invalid overlapping bank statement test inputs.
- Prior unverified/NOT RUN annotations below were written before #503 and are **superseded for the code at verified SHA**. Documentation-only commits or any future code change require their own CI. Whole financial/enterprise statutory close, target-module wiring, external bank proof and rate approval still pending.

## ACCOUNTING-001F4C3B2 — versioned foreign bank cutoff correction and reapproval
- **CODE IMPLEMENTED DIRECTLY ON main; no successful .NET compile, GitHub CI, EF migration execution or real PostgreSQL test result has been observed.**
- New additive migration `20261014000700_VersionedForeignBankCloseCorrections` creates immutable tenant-linked `bank_fx_revaluation_amendments`, `bank_fx_amendment_preparations`, `bank_fx_amendment_approvals` with unique version and journal lineage, FK, arithmetic and audit triggers. Existing v1 immutable valuation/approval unchanged.
- Protected `POST /api/v1/banking/statements/{id}/amend-foreign-revaluation` requires accounting.write, accounting.post and organizations.manage. Takes expected version/journal, original ledger base, native closing, exact cutoff ExchangeRate and new gain/loss accounts. Bank-account FOR UPDATE then exclusive fiscal year/period locks; requires original period Open, latest bank statement (no later GL/statement/revaluation dependencies), reconciled existing native/GL bridge and exact existing GL target. Creates reversed prior System journal, exact compensating Posted journal and numbered vN+1 replacement System journal (or zero-delta evidence only) atomically with append-only amendment and actor/reason.
- Foreign close evidence now picks latest amendment version, verifies old Reversed journal/compensating GL and new Posted journal and rate target; earlier approvals no longer count as current. Existing prepare/approve routes use **version-specific immutable maker/checker tables**, new SHA256 fingerprint with amendment identity and a different reviewer; original v1 fingerprint remains unchanged. Original `RevalueForeignBankAsync` cannot replay outdated v1 as though current. Generic foreign base-vs-GL report stays conservatively not finalized.
- **Nine new authenticated HTTP + migrated PostgreSQL integration test methods authored, NOT RUN**: exact inverse + new rate/v2 ledger and replay, v1 approval invalidation and v2 independent maker/checker, wrong version/rate/base/journal, closed fiscal period, cross-tenant, concurrent idempotent amendment, prior zero-delta valuation and a v3 chain returning to zero, and generic reversal guard covering all vN journals and compensating reversals. Existing ExchangeRate unique date/pair index requires updating the original master rate while historical v1/v2 snapshots remain unchanged. [Versioned close spec](../accounting-versioned-fx-bank-corrections.md).
- **Not fully complete ERP Accounting:** rate-master independent approval, statutory signatures, formal closed-year reopening, source bank statement correction, multi-period restatements, all-module financial closure, complete migrations and green CI remain pending.

## ACCOUNTING-001F4C3B1 — protected fee and unvalued realized FX GL corrections
- **IMPLEMENTED IN CODE, UNVERIFIED BY .NET COMPILATION, CI OR MIGRATED POSTGRESQL.** New raw migration `20261014000600_BankPostingCorrections`: immutable tenant-composite FK evidence for BankFee/ForeignSettlement line, original journal, reversal journal, actor, reason and date; unique source/reference/compensation constraints.
- Secured `POST /api/v1/banking/statements/{id}/lines/{lineId}/correct-posting`, accounting.write + accounting.post. Requires exact existing tenant bank fee/realized FX source journal, its original reference and posting date, distinct reversal/recording, mandatory reason and chosen original still-open correction period (no cross-period reversal); bank account FOR UPDATE → line FOR UPDATE → GL year/period locks → original journal lock. Draft inverse entries + numbered Posted System reversal + MarkReversed original + append-only correction audit all within one transaction. Same request replay idempotent, altered terms conflict.
- Foreign source correction **blocked** if an immutable FX revaluation or maker-prepared close covers this original movement. No mutation of prepared/approved bank evidence, no unapproved revalued period restart. Generic journal reverse cannot re-reverse the compensating bank correction. Statement details return correction reversal journal ID; bank report marks original reversed source **unmatched**, not falsely reconciled.
- **Eight additional real authenticated HTTP + migrated PostgreSQL regressions written, not run** (fee reversal/balance/report, replay/change, unvalued FX reversal, valued FX refusal, cross-tenant/bad source, closed period, direct/re-reverse generic route, parallel idempotency). No successful .NET CI or migrated DB run has been observed. See [correction scope](../accounting-bank-posting-corrections.md).
- **F4C3B2** correction of immutable revaluation/approved maker-checker close is deliberately **NOT implemented**; requires versioned source/journal correction & reapproval. `ACCOUNTING-001F` and full enterprise close remain IN PROGRESS.

## ACCOUNTING-001F4C3A — internal native-base bank close bridge and dual-review
- **CODED ON main; .NET BUILD, EF MIGRATION RUN, CI AND POSTGRESQL TEST EXECUTION NOT VERIFIED.**
- New tenant-safe immutable PostgreSQL migration `20261014000500_ForeignBankCloseApprovals`: original preparation/maker/fingerprint/note and distinct checker approval in separate append-only tables with tenant FKs and unique statement/preparation keys.
- `GET /api/v1/banking/statements/{id}/foreign-close-evidence` requires accounting.read; computes zero-opening native bank chain, validated Posted FX settlement journal account/sign and date, approved original cutoff rate/revaluation, correct Posted revaluation journal, only recognized bank GL movements, and actual cumulative base GL minus stored target base. Returns itemized issues and internal `internallyReconciled` bool, without directly comparing foreign/native balance to base-currency GL.
- Protected `POST .../prepare-foreign-close` requires accounting.write + accounting.post; `POST .../approve-foreign-close` requires accounting.post + organizations.manage and **different authenticated user**. Locks tenant bank account then fiscal-year and period **FOR UPDATE** (exclusive against normal posting FOR SHARE), recomputes current evidence, stores immutable SHA-256 fingerprint and audit notes; changed evidence, same maker/checker, no revaluation and repeat with changed terms rejected. Only when unchanged does internal approved flag become true.
- **Five additional real authenticated HTTP + migrated PostgreSQL integration tests authored, NOT EXECUTED**: reconciled USD→EUR bridge, 2-user maker/checker and replay, no pre-revaluation preparation, invalidated GL evidence, tenant isolation. **No running build/CI status found**. See [design](../accounting-foreign-bank-close-review.md).
- **F4C3B controlled corrections, independent statutory closing/signatures, nonzero opening migrations, bank authenticity and full finance-wide approval remain pending.** Bank approval is internal/statement-specific, not enterprise certification.

## ACCOUNTING-001F4C2 / foreign bank unrealized FX valuation
- **CODED ONLY — .NET SDK unavailable in this execution container; .NET BUILD, EF MIGRATION, CI AND REAL POSTGRESQL TEST EXECUTION UNVERIFIED.** Migration `20261014000400_BankForeignExchangeRevaluations` stores immutable, unique tenant bank/cutoff FX valuation including optional zero-delta journal, exact rate ID+snapshot, native closing, old base GL, new base target and P&L delta.
- Secured `POST /api/v1/banking/statements/{statementId}/revalue-foreign` requires accounting.write + accounting.post and exact expected native and base GL amounts. Reject base currency/negative overdraft, missing or undated base→foreign rate, non-analytic/non-active Revenue/Expense accounts, non-end-of-period or closed fiscal window.
- Under tenant bank row lock + fiscal period posting locks, validates zero-opening contiguous native statement chain, reconciles each imported native line to unique original Posted foreign bank System GL source, refuses unknown/reversed GL movements and unapproved migrated balances, checks cumulative posted bank GL base, then calculates target=`Round(nativeClosing / cutoffRate,4,AwayFromZero)` and DR bank / CR FX Revenue on uplift, DR FX Expense / CR bank on writedown. Atomic Posted journal + immutable audit evidence; for exactly zero delta stores evidence without creating zero GL journal. Same request replay returns original, changed request conflicts.
- Previously valued bank account blocks retroactive new bank statement import and native settlement posting for earlier cutoff dates. Generic `bank-fx-revaluation:v1:` journal reversal protected. **9 additional authenticated HTTP/migrated PostgreSQL tests written, not executed**: gain, loss, zero, unmatched source, wrong FX rate/base expected, tenant/closed window, backdated import, overdraft exclusion and concurrent idempotency.
- Still **not a fully certified multicurrency bank reconciliation**; foreign statement and base GL not directly comparable, so existing report retains full-close false until F4C3 reconciliation bridge and independent audit approval. More: [F4C2 scope](../accounting-foreign-bank-revaluation.md).

## ACCOUNTING-001F4C1 — approved foreign bank settlement and realized FX (UNVERIFIED)
- **CODED IN MAIN. .NET BUILD, CI AND MIGRATED POSTGRESQL TESTS HAVE NOT BEEN EXECUTED OR VERIFIED.** New PostgreSQL migration `20261014000300_BankForeignExchangeSettlements` stores immutable tenant-owned original bank line ↔ GL journal, approved Fx rate ID and 10-decimal rate snapshot, clearing base carrying amount, booked bank base value and realized FX gain/loss.
- Active tenant foreign bank account creation requires active currency configuration. Authorized `POST /api/v1/banking/statements/{statementId}/lines/{lineId}/post-foreign-settlement` posts journal against existing explicitly selected Asset/Liability clearing account: foreign native amount divided by dated approved base→foreign quote rate, DR/CR bank Asset, opposite clearing carrying amount, balancing FX Revenue gain or FX Expense loss. All unique, dated, approved account/type and fiscal-year/period controls and account/line row locks checked. Original accounting journal numbered and linked within same transaction. Replay returns original immutable rate+journal.
- Base-currency fee/payment match and auto-suggestions reject foreign currency bank accounts and FX-settled lines; generic journal reversal protects FX source. Statement details expose foreignSettlementJournalEntryId. Bank reconciliation counts only GL-verified posted foreign lines; reports foreign `statementVsLedgerDifference=null`, `bankAndLedgerCurrencyComparable=false`, `fullyReconciled=false` instead of subtracting foreign/native balance from base ledger.
- **Six** additional actual authenticated HTTP + migrated PostgreSQL tests authored, NOT EXECUTED: incoming gain, outgoing loss, malformed/changed replay, legacy base-currency route rejection, closed fiscal period/cross-tenant security, concurrent one-winner posting. See [realized FX details](../accounting-foreign-bank-settlements.md).
- **F4C2 unrealized period-end bank balance FX revaluation and F4C3 native balance reconciliation remain PENDING**, as do FX bank fees, controlled FX correction, historical signed bank-close evidence and target Banking module EF integration.

## ACCOUNTING-001F4B — bank fee GL posting (CODED; BUILD/CI UNVERIFIED)
- New PostgreSQL migration `20261014000200_BankFeePosting` creates immutable tenant-scoped fee-to-statement-line/GL journal link with composite foreign keys and uniqueness.
- Authorized `POST /api/v1/banking/statements/{statementId}/lines/{lineId}/post-fee`: exact negative bank amount, reason and active tenant analytical Expense account. Under bank/line locks and fiscal-year/period posting locks, atomic numbered System journal posts DR Expense / CR bank Asset and links fee evidence. Replays return same journal; changed details and closed periods conflict.
- Generic reversal of bank-fee journal is blocked; manual payment matching and auto-suggestions exclude fee lines. Statement detail includes fee journal ID and reconciliation validates posted fee bank credit separately from matched payments.
- **6 new full HTTP/migrated PostgreSQL regression methods written, NOT EXECUTED.** No validated .NET build, CI or migrated database run. Bank FX, VAT policy, controlled corrections and signed approvals PENDING. See [fee spec](../accounting-bank-fee-posting.md).

## ACCOUNTING-001F4A / bounded CSV import and unambiguous auto-match
- **CODE AUTHORED IN MAIN; BUILD, GITHUB CI AND MIGRATED POSTGRESQL INTEGRATION SUITE NOT EXECUTED / UNVERIFIED.**
- `BankStatementCsvParser` handles normalized four-column UTF-8 BOM/CRLF/RFC4180-quoted CSV, ISO yyyy-MM-dd dates, dot-decimal signed amounts, 1-MiB and 10k-row bounds; delegates to F3 transactional/import SHA256 idempotency/statement arithmetic.
- `BankReconciliationService` now provides `ImportCsvStatementAsync`, `GetMatchSuggestionsAsync`, `AutoMatchAsync` behind secured API routes: exact sign, amount, posting/value date, Confirmed BankTransfer, base-currency rate 1, matching Posted original System GL bank-account journal, tenant, uniqueness across both bank-line and payment candidate sets; ambiguous matches are skipped and never guessed. Before each posting of a match, reuses existing transaction-guarded `MatchLineAsync`. Partial batch success is explicitly reported.
- **5 new real HTTP/migrated PostgreSQL tests and 4 pure parser unit tests written (9 total), NOT RUN**. Cases: quoted import + replay, invalid header/date/quote/locale, unique match and idempotent rerun, ambiguous double-line matches refused, cross-tenant access, plus BOM/quoting/field and CSV size limits.
- No banking fee expense posting, realized/unrealized FX, camt.053/MT940/bank feed adapters, audit signoff or full enterprise financial closure yet. Mark `ACCOUNTING-001F4A` code-only and `ACCOUNTING-001F4` / `001F` IN PROGRESS. See [CSV / auto-match spec](../accounting-bank-csv-auto-match.md).

## ACCOUNTING-001F3 / operational base-currency banking statement matching
- **IMPLEMENTED IN CODE — .NET COMPILATION, CI AND MIGRATED POSTGRESQL TESTS UNVERIFIED.** Raw SQL EF discoverable migration `20261014000100_BankStatementReconciliation` adds tenant-scoped immutable `banking.bank_accounts`, `bank_statements`, `bank_statement_lines` (unique payment match, dedicated GL account, foreign keys, triggers).
- `IBankReconciliationService` implemented via PostgreSQL transaction-backed repository/service. Authorized endpoints: POST bank account, import structured signed lines with arithmetic consistency and SHA-256 idempotency, exact BankTransfer payment-to-original posted GL bank-account match and GET read-only bank-vs-GL statement reconciliation (repeatable-read snapshot, unmatched bank lines and uncleared posted book payments). Current cancellation is detected in reports. Operations reject cross-tenant ID use.
- **9 new HTTP + real migrated PostgreSQL regression methods written, NOT EXECUTED** in `FinancialHttpPostgresBankReconciliationTests.cs`: match/GL success, idempotent import, invalid statements, mismatch/unmatched, tenant isolation, ledger account/currency constraints, uniqueness across statements, cancelled payment invalidates match.
- **MVP only:** base-currency and explicitly matched payments, structured input not raw bank files, no fee GL posting, missing bank feed/MT940/ISO20022 parser, multi-currency, formal sign-off or audited company-wide reconciliation. Fiscal full-finance flags remain false. See [bank reconciliation](../accounting-bank-statement-reconciliation.md).
- The target Banking module 36-module Domain scaffolds remain unwired to these operational SQL tables; `ACCOUNTING-001F` still IN PROGRESS.

## ACCOUNTING-001F2 / cumulative receivables and payables open items
- **IMPLEMENTED IN CODE ONLY — .NET COMPILATION, RELEASE CI AND REAL POSTGRESQL TEST EXECUTION NOT VERIFIED.** New authorized tenant/year-scoped `GET /api/v1/accounting/fiscal-years/{id}/open-item-reconciliation`; dedicated Application service, Infrastructure repository and DTOs. Cumulative historical posted/reversed GL control balances through year end are compared with open issued Sales/received Purchases, net of confirmed document-currency payment allocations. Uses the source document's original exchange-rate snapshot and 4dp AwayFromZero per allocation. Current and historically assigned AR/AP control accounts (configured rules + original GL system journal) are included, so manual GL changes, mismatched payments and migrated opening balances appear as differences.
- **11 additional real HTTP + disposable migrated PostgreSQL regression methods written, NOT EXECUTED**, in `FinancialHttpPostgresOpenItemTests.cs`: happy partial AR/AP settlement, manual unmatched AR and AP, overallocation, wrong payment counterparty, missing rules, FX invoice/allocation, tenant isolation, complete settlement, payment header/row mismatch and confirmed header allocation with zero child rows. Output includes per-document overdue days/aging, per-counterparty balances, and signed per-account GL deltas.
- Response deliberately declares `historicalAsOfSnapshotAvailable=false`, `bankStatementsReconciled=false`, `financialSubsystemsFullyReconciled=false`. Legacy status is current rather than event-time historical, and Banking is still scaffolded; this is **not** signed historical AR/AP aging, cash clearing, bank matching or statutory close certification. No new EF module migration. [Scope and limitations](../accounting-open-item-reconciliation.md).
- `ACCOUNTING-001F` remains **IN PROGRESS**; 001F3 needs operational Banking, later credit notes/returns and true historical snapshot events. Do not claim full finance reconciliation or passing tests.

## ACCOUNTING-001F1 / cross-subsystem source ↔ journal integrity
- **CODE IMPLEMENTED, .NET BUILD / CI / MIGRATED POSTGRESQL TESTS UNVERIFIED**. New `IFinancialSourceReconciliationService`/`Repository` and `GET /api/v1/accounting/fiscal-years/{id}/source-reconciliation` behind `accounting.read`. Tenant/year scoping and a PostgreSQL year transaction lock.
- The read-only report checks *current* issued Sales, received Purchases and confirmed Payments against one original `System` journal per external source reference; canceled sources must have a posted reversal with account/line numbers and exact debit/credit inversion; draft sources must not have a posted journal. Compare source gross amounts using the same base-currency conversion as AutoPostingEngine; check system control line amounts, balanced entries, orphan/missing references, payment allocation totals/directions.
- **11 new HTTP+disposable migrated PostgreSQL test methods written but NOT EXECUTED** in `FinancialHttpPostgresSourceReconciliationTests.cs` (10) and `FinancialHttpPostgresInvoiceReconciliationTests.cs` (1). Scenarios: empty state/banking not ready, confirmed payment missing/matching/incorrect original GL, orphan GL, missing original Sales/Purchases, draft/tenant/anonymous, payment allocation mismatch, valid exact reversal, balanced but incorrect reversal.
- This is **ACCOUNTING-001F1 only**. No actual Banking module, bank statement matching, AR/AP aged open-item/control-balance reconciliation, historical as-of reports, tax/stock/payroll/consolidation or financial sign-off. `financialSubsystemsFullyReconciled` and `bankingReconciliationAvailable` are always **false**. See [source-ledger reconciliation](../accounting-source-ledger-reconciliation.md).

## ACCOUNTING-001E / controlled correction and base GL reconciliation
- **CODE IMPLEMENTED; CI, .NET compilation, PostgreSQL execution UNVERIFIED.** Old fiscal-year `reopen` rejects closed years with prior P&L closing journal; normal journal reversal rejects both year-end closing entries and their linked compensating reversal entries. New `POST .../reopen-profit-loss` requires `expectedClosingJournalEntryId`, 15–250 character reason and both accounting write/post; reverses the closing journal with immutable linked numbered compensating entry while reopening final period/year atomically. Identical retry returns same reversal; changed reason or stale closing ID -> 409. Old entries immutable.
- `ProfitLossClosingService.CloseAsync` now supports versioned reclose `:r2`, `:r3` after legitimate correction; year locks protect concurrent close/reopen. New authorized `GET .../close-reconciliation` reports GL posted debit/credit totals, P&L balances, drafts, calendar readiness and latest closing id (aggregate GL only, not a full audited statutory reconciliation).
- Six additional HTTP + migrated PostgreSQL tests written in 2 partial-class test files; **not executed**, including close/reopen/reclose, exact reversing lines, reason mismatch, stale retry after reclose, tenant isolation, concurrent one-winner, reconciliation before/after, and draft count. See [year-end corrections](../accounting-year-end-corrections.md).
- No AP/AR, bank, inventory, tax, payroll, FX, consolidation, next-year carry-forward or signed closing workflow validation; no new Accounting module EF migration.

## ACCOUNTING-001D / actual P&L year-end journal and net income transfer
- **IMPLEMENTED IN CODE — BUILD/CI/POSTGRESQL TEST EXECUTION UNVERIFIED**.
- New `ProfitLossClosingService`, `IProfitLossClosingService`, `ProfitLossAccountBalance` and `IJournalRepository.GetProfitLossBalancesAsync` aggregate posted and reversed tenant income/expense amounts for the exact fiscal-year dates. New `POST /api/v1/accounting/fiscal-years/{id}/close-profit-loss` requires `accounting.write` and `accounting.post` and an explicit analytical retained-earnings equity account.
- Under an exclusive year row lock: require open year, contiguous periods, prior periods Closed, final period Open, no drafts or misdated period journal entries; create system journal reversing each nonzero P&L account net balance, transfer net income/loss to equity, post the journal and Close the final period and year **in a single transaction**. Unique existing system reference `profit-loss-close:v1:{id:N}` ensures one effect and replay returns the same ID. No migrations.
- Existing status-only final-year Close now rejects outstanding income/expense account balances. Eight **new, unexecuted** migrated PostgreSQL HTTP regression tests cover profit/loss/zero-result, duplicate and concurrent close, tenant/invalid destination/drafts/calendar failures and manual Close bypass.
- **Not** statutory year-end sign-off, tax, group consolidation, depreciation, bank/AP/AR/inventory reconciliations, next-year carry-forward, full module migration, or verified production feature. Details: [year-end profit/loss](../accounting-year-end-profit-loss.md).

## ACCOUNTING-001C / transactional accounting close controls
- **IMPLEMENTED, CI/.NET/real PostgreSQL execution NOT YET VERIFIED**. Active legacy `FiscalCalendarService` now closes/reopens periods/years within `ITransactionRunner` transactions using year-then-period exclusive PostgreSQL row locks; rejects unresolved Draft journals and period-misdated journals; requires all child periods closed before year hard close, no open periods before year soft close.
- Fiscal year/period edits and journal draft create/update/line mutations now acquire posting/calendar locks, recheck period/year after waiting and reject edits in closed/soft-closed periods. Six fiscal status-transition endpoints additionally require `accounting.post` alongside `accounting.write`.
- 10 new real HTTP + migrated PostgreSQL tests added: draft close gates, soft close/reopen, final year close and child reopen, year soft-close state, cross-tenant rejection, blocking lock contention, closed-period draft mutations, stranded-draft year-close denial, period shrink denial over posted dates, and rejection of calendar changes after year close. These test cases have **NOT YET BEEN RUN**.
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
