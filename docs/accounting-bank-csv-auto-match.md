# ACCOUNTING-001F4A — safe CSV bank import and unambiguous automatic payment matching

**2026-10-14 | Code authored on main | .NET compile, CI and PostgreSQL integration tests NOT VERIFIED.**

This is a bounded implementation of `ACCOUNTING-001F4`, extending the
operational `ACCOUNTING-001F3` BankReconciliationService on the active
legacy .NET 10 Web API. It does **not** finish the entire F4 fee/FX/approval scope.

## New routes

Authenticated, tenant-scoped under `/api/v1/banking`:

- `POST /accounts/{accountId}/statements/import-csv`: requires **both**
  `accounting.write` and `accounting.post`. The JSON request contains
  `externalId`, `fromDate`, `toDate`, `openingBalance`, `closingBalance`,
  and `csv` as **UTF-8 CSV text**, NOT base64.
  The CSV header is exactly `externalId,valueDate,signedAmount,description`.
  `valueDate` is `yyyy-MM-dd`, `signedAmount` uses invariant dot decimal,
  receipts are positive and payments negative. CRLF, LF, BOM and RFC4180
  escaped quoted descriptions are accepted. Delimiter is comma; semicolon,
  bank-specific exports and Excel locale conversions are deliberately not
  guessed. Limit **1 MiB** and **10,000 rows**; invalid headers, excess
  columns, broken quoting, malformed dates/amounts are rejected.
  The parser feeds the existing transactional import, preserving duplicate
  line/date/precision checks, opening + sum(lines) = closing, immutable
  imported rows and canonical SHA256 idempotent replay.

- `GET /statements/{statementId}/match-suggestions`: requires
  `accounting.read`. Returns unmatched-line count and **only unambiguous
  one-to-one** `(statementLineId, paymentId, amount, date)` suggestions.
  A proposed payment must be currently Confirmed, BankTransfer,
  base-currency exchange rate exactly 1, same tenant, same **value date**,
  same signed amount and payment direction, not already matched on *any*
  imported bank statement line, and have exactly one Posted System
  `payment:{paymentId:N}` journal line on this bank's dedicated GL account
  with the same signed amount and posting date. Both directions must be
  unique across the entire candidate graph for the statement. Two equal
  lines for one payment, or two equal payments for one line -> **NO
  automatic suggestion**; do not choose by sorting/proximity.

- `POST /statements/{statementId}/auto-match`: requires both
  `accounting.write` and `accounting.post`. Recomputes the exact
  suggestions, then passes each one to the existing transaction-locked,
  tenant-verified `MatchLineAsync`. That method still enforces current
  paid status, sign, account, journal and database uniqueness before
  committing an actual match. An individual stale/raced conflict leaves
  that line unmatched and is recorded as skipped, not guessed. The
  response reports `proposed`, `matched`, `alreadyMatched`, `skipped`
  and `skippedLineIds`. Each line commits independently; **batch partial
  success is possible** and caller must review skipped and outstanding
  lines. Repeating the command will not create duplicates.

No new tables or migration are required for this slice; it uses the F3
`banking` tables and existing immutable match link.

## Tests written (unverified)

- Five new full authenticated HTTP + disposable migrated PostgreSQL test
  methods in `FinancialHttpPostgresBankAutomationTests.cs`: CSV quoting and
  replay; invalid formats and malformed quotes; exact posted-bank-payment
  suggestion, match and safe repeat; ambiguous two-line-to-one-payment case
  refuses automatic matching; foreign organization cannot suggest or auto
  match another organization's statement.
- Four additional pure parser unit test methods in
  `BankStatementCsvParserTests.cs`: BOM/comma/escaped quoting, locale-
  dependent decimal format rejection, broken quotes/column counts, and
  over-1-MiB input.
- **Compilation / execution status is UNKNOWN; do not call them passing**
  until .NET 10 CI compiles, creates migrated PostgreSQL DB and runs them.
  Record build outputs and actual GitHub run IDs in `docs/ai/VALIDATION.md`.

## Remaining F4B/F4C / full ACCOUNTING-001F

- **Actual bank fees:** approved fee-category mapping, legitimate bank
  expense/asset GL entry, idempotent journal source reference, VAT/tax and
  reversals, fee split/aggregate matching, period/year posting locks.
  **Not implemented; never auto-post fee expense from arbitrary CSV text.**
- **Foreign-currency bank statements:** actual account currency, payment
  currency, settlement-date FX snapshot, realized differences to configured
  FX gain/loss accounts and period-end unrealized revaluation.
  **Not implemented; F3 still refuses bank account currencies different
  from the tenant base currency.**
- **Real bank feed protocols:** structured CSV format is proprietary to
  Fintrox, not ISO20022 camt.053, MT940 or OFX parsers or an open banking
  connection. Bank-specific dialect mapping, signed originals and persistent
  raw file/audit uploads remain pending.
- **Finance close:** statement opening-balance continuity across periods,
  signature and four-eyes approval, linked book/bank reconciliations,
  unmatched accrual/fee issue workflow, independent close checklist, and
  final enterprise-wide accounting attestation remain pending.
- `financialSubsystemsFullyReconciled` in fiscal reports **stays false**.
  Target new Banking-module Domain scaffolds remain **unwired** to the
  operational legacy table mapping.

Next implementation milestone: `ACCOUNTING-001F4B` — explicit, authorized
and balanced bank-fee accounting with correct fiscal-period controls,
followed by `001F4C` FX statements and signed bank-close workflow.
