# ACCOUNTING-001G1 — monthly recurring journal templates and manual generation

2026-10-15. **CI VERIFIED:** GitHub #522 (https://github.com/viktor132607/Fintrox.Server/actions/runs/38658049296), source SHA `a7544869081888e7c090c9abba32919cb3316e4b`. All **3036 tests passed**, EF snapshot clean, PostgreSQL migration applied, Docker published.

## Bounded functional scope

A recurring journal template is an immutable balanced GL instruction set. The first slice
supports a **monthly schedule on days 1–28** (avoids ambiguity from short months),
a first posting date, an optional inclusive last date, a description/name and 2–100
fixed debit/credit lines. It is tenant-scoped and validated against currently active,
analytical accounts allowing manual posting; create rejects cross-tenant, unbalanced,
negative, mixed debit/credit, overprecise or missing accounts.

An authorized accountant can **generate one System Draft for a specified scheduled
posting date**. Generation does **not** post the journal, allocate a GL posting
number, or trigger an asynchronous background job. The generated draft has a
source reference of the form `recurring-journal:v1:{templateId:N}:{yyyyMMdd}`
and contains a full snapshot of the original template lines. Ordinary manual
draft editing endpoints cannot alter that generated source-controlled draft.

Review and final `POST /api/v1/accounting/journal/{journalId}/post` are separate,
protected by existing `accounting.post` rights, ordinary financial year/period
locks and GL constraints. Deactivating an account between template creation and
generation blocks generation; changing the template requires creating a new template.

## API (authenticated)

- `GET /api/v1/accounting/recurring-journals`: `accounting.read`, tenant-only list.
- `POST /api/v1/accounting/recurring-journals`: `accounting.write`, creates an immutable monthly template (201).
- `POST /api/v1/accounting/recurring-journals/{id}/generate`: `accounting.write`, returns the existing/generated journal ID (200) and `alreadyGenerated`.
- Posting remains `POST /api/v1/accounting/journal/{journalId}/post`, requiring `accounting.post`.

New discoverable raw migration
`20261015000100_RecurringJournalTemplates` creates normalized
`accounting.recurring_journal_templates`,
`accounting.recurring_journal_template_lines` and
`accounting.recurring_journal_generations`, with composite tenant
foreign keys, positive one-sided debit/credit checks, unique source date,
unique generated journal, reason/line bounds and PostgreSQL UPDATE/DELETE
rejection triggers for immutable template/generation audit evidence.

## Concurrency, idempotency and posting safety

1. Scope the requested template to current organization and lock its row
   `FOR UPDATE`, serializing concurrent requests for the same template.
2. Validate the requested date against day of month, first date and optional
   last date. If the immutable source generation already exists for that
   exact date, return its current journal status/ID even if the fiscal
   period later closes. Never silently generate a duplicate.
3. Lock the posting fiscal year then period using the same GL posting
   locks as `JournalService`. Check both remain Open; reject missing or
   changed periods.
4. Verify all accounts still belong to the organization and are active,
   analytical and posting-eligible, and verify the template balances.
5. Create System Draft+lines and insert the unique source-generation
   evidence within one EF/PostgreSQL transaction. Failed requests roll
   back both journal and audit link. Separate GL posting is mandatory.

The slice deliberately does **not** implement automatic scheduling, annual/
weekly/last-day schedules, template revisions/amendments, dynamic expressions,
tax recognition, approval workflows, posting queues, accruals, deferrals
or automatic catch-up for missed months. Those remain `ACCOUNTING-001G2`
and `ACCOUNTING-001H` / future enterprise accounting work.

Five real migrated-PostgreSQL HTTP tests authored and executed successfully:
balanced Draft and separate Posted transition, invalid/cross-tenant
accounts and invalid schedules, closed period, tenant boundary and parallel
generation idempotency.

**Not evidence of statutory financial close.** Do not mark ACCOUNTING-001 complete.

## ACCOUNTING-001G2A — read-only schedule monitoring (2026-10-15)

Added `GET /api/v1/accounting/recurring-journals/{id}/schedule?from=YYYY-MM-DD&to=YYYY-MM-DD&asOf=YYYY-MM-DD`.
Requires `accounting.read`. Both bounds are inclusive, maximum **366 days**;
`asOf` is optional (defaults to current **UTC** date; callers should pass
their chosen business date for local accounting calendars).
Missing or other-tenant templates return 404. Invalid windows and corrupted
generation/journal references return a typed 409. No new migration.

Every scheduled day 1–28 within the immutable template effective dates is
projected as `Scheduled`, `Due`, or `Overdue` relative to `asOf`.
Existing immutable generation evidence is joined to the actual GL journal
and reports `Draft`, `Posted` or `Reversed`, journal ID and journal status.
The query is read-only and never generates, edits, posts, catches up or
reschedules transactions. PostgreSQL provides a consistent snapshot for
the single joined query; reporting does not replace the fiscal close controls.
Three additional migrated-PostgreSQL authenticated HTTP tests cover
state transitions and side-effect freedom, date cutoff and tenant isolation,
and bounds/missing templates. **CI VERIFIED:** [GitHub #526](https://github.com/viktor132607/Fintrox.Server/actions/runs/38772495791) on code SHA `f5d231d6ab9ee08a44205ff70731fa27a2b85a82`: Release build 0 warnings/errors, **3039/3039 tests passed** (0 failures, 0 skipped), clean EF model snapshot, PostgreSQL migrations applied/listed and Docker Build & Publish passed.

Still pending in `ACCOUNTING-001G2`: immutable versioned template
revisions, explicit scheduler/worker with recoverable leases and execution
audit, and policy-controlled notifications. Nothing automatically posts.

## ACCOUNTING-001G2B — immutable versioned templates (2026-10-16)

New authorized routes:
- `POST /api/v1/accounting/recurring-journals/{id}/revise`
  (`accounting.write`, 201). Body:
  `{"effectiveFromMonth":"2026-04-01","replacement":{"name":"Monthly rent v2","description":"Revised rental payment","firstPostingDate":"2026-04-20","lastPostingDate":null,"lines":[{"accountId":"...","debit":250,"credit":0,"description":"Rental expense"},{"accountId":"...","debit":0,"credit":250,"description":"Bank"}]}}`
- `GET /api/v1/accounting/recurring-journals/{id}/versions`
  (`accounting.read`, tenant-filtered list of root, previous/current
  template version IDs, first effective month and superseding cutoff).

The predecessor and all generated GL source snapshots remain immutable.
A revision creates a new template ID and an append-only predecessor /
replacement / root edge, with monotonically increasing version number.
DB constraints enforce tenant-owned composite foreign keys, one successor
per predecessor, one predecessor per replacement, and unique root+version.
The new template is activated on the **first day of the month**
(`effectiveFromMonth`); its first monthly posting must be in that month,
with a day between 1 and 28. The cutoff must be strictly after the
predecessor's first scheduled month. Revisions with a generation recorded
on or beyond the requested cutoff are rejected rather than rewriting
financial history, even if the original entry remains Draft or was reversed.
A further revision must target the **current leaf**, not an ancestor.

The predecessor row is locked `FOR UPDATE` in both revision and
manual generation. This serializes competing cutovers and generation
so an old version cannot generate any occurrence on/after its successor
month. Previously generated older-month entries remain replayable,
including after accounting period closure. The read-only schedule
endpoint excludes superseded months, while the new template has its own
monthly occurrences. No automatic journal posting or job runner.

Replacement lines pass the same debit/credit precision, positive-side,
balance, active/analytical/manual-posting eligibility and tenant checks
as G1. Insertion of the template, normalized lines, and lineage edge is
one transaction; a failure rolls back all three. Migration
`20261016000100_RecurringJournalTemplateRevisions` adds immutable
`accounting.recurring_journal_template_revisions` with guarded
UPDATE/DELETE and tenant FKs. The old template/GL and existing source
external reference format are preserved.

Five real HTTP/migrated-PostgreSQL tests cover versions, cutoff
schedules, historical idempotence, broken inputs, cross-tenant access,
and concurrent competing revisions. **CI VERIFIED:** [GitHub #531](https://github.com/viktor132607/Fintrox.Server/actions/runs/38808420273) at application source SHA `3444b68430cfca6509e39dc090f70c392f46ce8d`: Release build 0 warnings/errors; **3044/3044 tests passed, 0 failed/skipped**; EF snapshot clean, migration `20261016000100_RecurringJournalTemplateRevisions` successfully applied/listed on PostgreSQL; Docker build/publish successful.

Remaining `ACCOUNTING-001G2C`: explicit opt-in scheduled generation,
durable retry/lease and operations monitoring. Existing manual
`generate` only creates Draft; `accounting.post` is still a separate
authorized action. `ACCOUNTING-001H` accruals/deferrals and statutory
accounting close remain out of scope.

## ACCOUNTING-001G2C — opt-in durable UTC draft automation (2026-10-16)

`PUT /api/v1/accounting/recurring-journals/{id}/automation`
body `{"enabled":true}` or `{"enabled":false}`
(`accounting.write`) explicitly enables/disables one template,
recording the currently authenticated user. Missing/other-tenant
template is 404; already superseded versions cannot be newly enabled.
Replacement versions must be enabled separately. Read endpoints
`GET .../{id}/automation` (configuration if opted) and
`GET .../{id}/automation/runs` (last 200 run statuses) require
`accounting.read`.

Opt-in is **disabled by default**. Every minute, a hosted worker
checks the **current UTC accounting date**, queues only that day's
active eligible templates (days 1–28, effective range, not superseded)
and claims up to 10 due jobs. The worker does not perform historical
catch-up outside of explicitly stored retries; max job lookback is 7
days. The polling loop is controlled by
`RecurringJournals:EnableWorker` (default true).
The system does **not** call `accounting.post`; the created System
journals remain **Draft** until explicitly posted by authorized staff.

PostgreSQL migration `20261016000200_RecurringJournalAutomation`
adds per-template opt-in, unique tenant/template/date runs with
status Pending/Claimed/Retry/Generated/Failed, 2-minute leases,
max 5 attempts and 1/5/15/60-minute backoff, with append-only
Claimed/Generated/Retry/Failed audit events. `FOR UPDATE SKIP LOCKED`
atomically selects jobs across API instances; conditional
lease-token CAS prevents old workers from writing outcomes for
reclaimed leases. A crash after journal insertion but before
completion is safe because the original GL source generation remains
uniquely keyed, replaying to the same journal. A worker can only
generate for the user who enabled the current schedule while that
user and organization are active and accounting.write remains held.
Opt-out is serialized with generation under a PostgreSQL share lock.
Failures never bypass financial-period, account or tenant gates.

Five real migrated PostgreSQL HTTP/worker tests cover opt-in, draft-only,
opt-out/tenant boundaries, concurrency, lease recovery and
closed-period retries. **CI VERIFIED:** [GitHub #539](https://github.com/viktor132607/Fintrox.Server/actions/runs/38812934895), application SHA `0234425a09894690c5493d72cd1529e8b81471f4`: 3049/3049 tests passed (0 failed/skipped); Release build zero warnings/errors, EF snapshot clean, migration `20261016000200_RecurringJournalAutomation` applied/listed in PostgreSQL, Docker Build & Publish passed. Identity FK was corrected to `identity.users("Id")` after CI #538.

Still outside G2C: automatic GL posting, arbitrary cron,
timezone-local calendar scheduling, historical mass backfills,
external notifications, formal maker/checker approvals and accruals.
