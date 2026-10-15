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
and bounds/missing templates. **CI/build validation pending for G2A.**

Still pending in `ACCOUNTING-001G2`: immutable versioned template
revisions, explicit scheduler/worker with recoverable leases and execution
audit, and policy-controlled notifications. Nothing automatically posts.
