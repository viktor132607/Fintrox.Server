# ACCOUNTING-001F4C3B2 — Versioned corrected bank FX cutoff and independent reapproval

**2026-10-14 | CODED IN MAIN; .NET BUILD, POSTGRESQL MIGRATION EXECUTION, CI AND FULL INTEGRATION TESTS ARE UNVERIFIED.**

## Why append-only versioning

Original bank statements, realized FX settlements, original unrealized FX
revaluation, its Posted GL journal and all previous maker/checker approvals
remain immutable. An approved FX cutoff cannot be corrected by editing a
rate, deleting the first revaluation or reversing its bank GL without a
replacement. This bounded workflow generates an explicit **new cutoff
version**, an inverse of the old FX P&L and bank entry, a new balanced
replacement valuation, and a **fresh independent maker/checker review
for that same version**. The generic journal reversal route protects **every** `bank-fx-revaluation:vN:` source and its linked compensating reversal; only the controlled amendment API can change this accounting chain. Earlier review evidence stays in the database but
never counts as approval of a subsequent version.

## Secure API

`POST /api/v1/banking/statements/{statementId}/amend-foreign-revaluation`

Requires authenticated tenant membership and **all three** permissions:
`accounting.write`, `accounting.post`, `organizations.manage`.

Example:

```json
{
  "expectedCurrentVersion": 1,
  "expectedCurrentJournalEntryId": "00000000-0000-0000-0000-000000000001",
  "exchangeRateId": "00000000-0000-0000-0000-000000000002",
  "fxGainAccountId": "00000000-0000-0000-0000-000000000003",
  "fxLossAccountId": "00000000-0000-0000-0000-000000000004",
  "expectedNativeClosingBalance": 120.0000,
  "expectedLedgerBaseBalance": 100.0000,
  "reason": "Independent audit found an incorrect month-end rate"
}
```

When the previous approved valuation was **exactly zero** and therefore had
no journal, `expectedCurrentJournalEntryId` is `null`.

The `ExchangeRate` master enforces **one record per organization/currency pair/effective date**. A corrected cutoff rate therefore uses the existing dated rate record (whose old book rate remains preserved in the immutable v1 snapshot), not an illegal second row for the same pair/day. The version is an explicit compare-and-swap precondition. Different
inputs, wrong original journal, nonmatching rate/date/currency/account
or unexpected bank GL balance -> **409**, foreign-tenant source -> **404**.
An identical replay with the same maker and original terms returns the
previous amendment without duplicate GL journals.

## Versioned accounting flow

1. Find an already stored foreign bank Asset cutoff and original
   `bank_fx_revaluations` snapshot. Reject overdrafts, a changed native
   closing, a subsequent bank statement, subsequent FX valuation or
   **any later Posted/Reversed bank Asset GL movement**. The workflow
   intentionally does not attempt an unapproved cross-period restatement.
2. Acquire tenant bank account `FOR UPDATE`, then its fiscal year
   `FOR UPDATE` and period `FOR UPDATE` **exclusively**. Standard
   journal postings take fiscal posting shared locks, so cannot race the
   replacement journal while its evidence is being verified. Fiscal period
   and year must still be **Open**, and cutoff must equal the period end.
3. Read latest version (v1 in original table, v2+ in immutable
   `bank_fx_revaluation_amendments`). Require exactly the expected
   original version/journal and original pre-valuation base GL. Verify
   current posted bank GL equals the previous version's target, and
   require the full native statement/FX settlement/revaluation evidence
   bridge still be internally reconciled.
4. Validate new exact-cutoff tenant-owned ExchangeRate with base→foreign
   currency convention and active analytical tenant Revenue gain and
   Expense loss accounts distinct from the bank Asset account.
5. Let `baseBefore` be the original GL amount *before the first cutoff
   valuation*, not the original final target. Compute
   `newTarget = Round(nativeClose / selectedRate, 4, AwayFromZero)`
   and `newAdjustment = newTarget - baseBefore`.
6. If prior adjustment nonzero, lock original/latest valuation journal,
   verify original posted System reference
   `bank-fx-revaluation:v{n}:{bankId:N}:{cutoff:yyyyMMdd}`, create
   a **numbered exact inverse System journal**, mark that source
   `Reversed` and link `ReversalOfJournalEntryId` / `ReversedByJournalEntryId`.
   If prior adjustment was zero, no dummy inverse is created.
7. If new adjustment nonzero, create another **numbered balanced System
   journal** with new reference `bank-fx-revaluation:v{n+1}:...`:
   uplift DR bank Asset / CR FX Revenue, writedown DR FX Expense / CR bank
   Asset. If zero, no dummy replacement entry is made. All journals
   and original/new metadata commit in the **same PostgreSQL
   transaction**.
8. Insert unique append-only `bank_fx_revaluation_amendments` record:
   previous/reversal/replacement journal IDs, statement+cutoff,
   previous target and adjustment, original base carrying amount, new
   cutoff ExchangeRate ID+rate snapshot, target and adjustment, accounts,
   actor, reason and sequential version. Original historical
   `bank_fx_revaluations` and approvals are **not updated or deleted**.

## New version of the bank close review

The existing three routes remain unchanged:

- `GET /api/v1/banking/statements/{id}/foreign-close-evidence`
- `POST /api/v1/banking/statements/{id}/prepare-foreign-close`
- `POST /api/v1/banking/statements/{id}/approve-foreign-close`

The evidence reader now chooses the **highest version** of the
immutable cutoff. It independently checks native→base totals,
Posted settlement GL links, exact current replacement bank GL and
gain/loss lines, prior reversed valuation GL links, authorized
compensations, and no unrecognized bank asset journals.

`versionNumber` is returned in current close evidence and the
maker/checker responses. The SHA-256 fingerprint for **v1 is kept
byte-for-byte compatible**, preserving old review evidence. Amendments
add their unique ID and version to the fingerprint. Once v2 exists,
the v1 maker/checker records remain historical but do **not** appear
as current `prepared` or `approved`.

Two new append-only tables record preparation and approval **by amendment
version**, with the same user/permissions, audit notes, evidence hash,
two-different-user maker/checker and fingerprint revalidation as the
initial v1 flow. The review recomputes evidence under tenant bank-account
and exclusive fiscal-year/period locks. A new maker and a different
checker must review v2 before `approved=true` can be returned for it.
Future amendments (v3+) repeat the process with independent version
records. There is no retrospective modification of old approval rows.

## Migration

`20261014000700_VersionedForeignBankCloseCorrections`

- `banking.bank_fx_revaluation_amendments`
- `banking.bank_fx_amendment_preparations`
- `banking.bank_fx_amendment_approvals`

Unique sequential version per tenant/statement, unique original,
reverse and replacement journal references, tenant-scoped foreign keys,
rate and line arithmetic check constraints, mandatory reason and
DB triggers preventing UPDATE/DELETE. Original v1 tables remain
untouched; migration is additive.

## Authored regressions — NOT executed

Nine new authenticated HTTP/migrated PostgreSQL test methods in
`tests/Fintrox.Domain.Tests/Accounting/FinancialHttpPostgresVersionedBankCloseTests.cs`:

1. EUR bank v1 target120 from USD120 @1 and initial EUR100;
   revised USD/EUR rate 1.5 reverses original EUR20 gain and posts EUR20
   loss as v2. Original Posted→Reversed, inverse and replacement Posted;
   final bank GL EUR80; full native/base bridge reconciled; identical
   replay returns same new journal.
2. Approved v1 review remains historical; v2 is explicitly unprepared
   and unapproved until a fresh maker and a different checker confirm
   the new evidence. Old checker cannot silently carry approval forward.
3. Wrong version, stale prior journal, incorrect base amount and
   unknown FX rate all return conflict.
4. Cutoff accounting period closed -> conflict.
5. Cross-tenant cutoff -> 404.
6. Two concurrent identical amended requests serialize to one GL
   reversal and one replacement version.
7. Previously zero-delta v1 valuation gets v2 adjustment without creating a fictitious reverse journal.
8. A second correction creates v3 and can return to zero delta without a dummy replacement journal.\n9. Generic GL reversal cannot reverse a v2 valuation journal or its previous protected bank valuation compensation.

These tests have **not been run**: no local `dotnet` SDK was found
and available current GitHub status does not establish a passing
pipeline. Do not label them green or use this for real production
accounting until the EF migration runs and full PostgreSQL integration
tests pass.

## Explicit exclusions

- Already-closed fiscal year/period restatement requires an authorized
  fiscal reopen and formal statutory compliance workflow; **refused**.
- A later bank statement, later valuation or later bank GL movement
  depending on the original balance requires coordinated multi-period
  correction; **refused** here.
- Bank **statement source correction** and replacing original
  AR/AP currency-clearing settlements are a different flow; this changes
  only the stored **unrealized cutoff FX valuation** rate and balanced GL.
- Signed external bank evidence, qualified financial signatures,
  official rate-master approval status and full enterprise-wide
  AR/AP/tax/inventory statutory closing remain out of scope.
- The existing generic banking report retains its conservative
  `fullyReconciled=false` for foreign balances. The **specific
  `foreign-close-evidence` route** provides the auditable native/base
  bridge and current-version approval status.
- `ACCOUNTING-001F` still IN PROGRESS; all accumulated 001F additions
  need a passing build, PostgreSQL migration and full test suite.
