# ACCOUNTING-001F4B — controlled bank fee GL expense posting

**2026-10-14 — CODE IMPLEMENTED IN LEGACY RUNTIME; .NET build, CI and migrated PostgreSQL tests NOT EXECUTED/VERIFIED.**

## Authorized API

`POST /api/v1/banking/statements/{statementId}/lines/{lineId}/post-fee`

Requires active tenant and **both** `accounting.write` + `accounting.post`.

```json
{
  "expenseAccountId": "00000000-0000-0000-0000-000000000001",
  "expectedSignedAmount": -5.0000,
  "reason": "Monthly bank account service fee"
}
```

The request is **explicitly initiated and authorized** by an operator, not guessed from imported bank text. The signed amount is an optimistic precondition protecting the original imported evidence. The selected expense account must exist in the active tenant, be **active, analytical and type Expense**. The imported bank line must have a **strictly negative amount** with at most four decimal places, and must not already match a payment. Reason is mandatory, 10–250 characters. There is **no automatic VAT deduction, VAT booking, withholding tax, FX conversion or unapproved expense category assignment** in this slice.

200 response: `BankFeePostingResponse` with `statementId`, `statementLineId`, `journalEntryId`, `expenseAccountId`, `feeAmount` (positive), `reason`, and `alreadyPosted`. Cross-tenant/not-found statement or line returns 404, invalid accounting state returns 409, missing permissions 401/403.

## Actual posting and finance controls

- New discoverable PostgreSQL migration `20261014000200_BankFeePosting` introduces immutable **`banking.bank_fee_postings`** referencing a statement line, organization-owned posted accounting journal and expense account. Tenant-composite foreign keys, unique original line and unique journal references prevent double ownership. PostgreSQL trigger rejects **updates and deletes**, maintaining an audit trail. No direct modification to imported `banking.bank_statement_lines`; its F3 immutability trigger still applies.
- `BankReconciliationService.PostFeeAsync` runs inside an execution-strategy PostgreSQL transaction and locks tenant **bank account first, then statement line** (`FOR UPDATE`), matching the order of payment-match operations. It validates amount, original unallocated status, existing fee ownership and exact idempotent replay.
- Before creating a new journal, resolve the fiscal period by the line's **bank value date**; acquire the existing GL **year then period `FOR SHARE`** posting locks. Re-read the year/period states under locks and reject closed/inconsistent ranges. Reject cross-tenant, inactive or non-analytical expense accounts, and expense == bank GL account.
- Reuse the **authoritative** legacy `IJournalRepository`, `JournalEntry.CreateDraft`, `JournalLine.Create` and the year-scoped `AllocatePostingSequenceAsync`. System external reference `bank-fee:v1:{lineId:N}` is unique per tenant. Persist draft **before** posting to respect the immutable journal-lines DB trigger. Balanced entry:
  - **Debit** the explicitly selected Expense account by the fee's positive amount.
  - **Credit** the dedicated bank Asset account by exactly the same amount.
  - A numbered Posted system journal and immutable `bank_fee_postings` evidence link commit in the same transaction; any failure rolls back the link, journal lines, status and number allocation.
- An identical request replay returns **the same journal ID** with `alreadyPosted=true`, even if the original period has since closed. A changed reason/account/signed amount, already matched payment, or invalid previous journal returns 409. Tenant bank-account locking serializes concurrent identical requests.
- Generic GL journal reversal of a `bank-fee:v1:` entry is blocked until a **separate controlled bank fee correction** workflow exists; otherwise a standalone reversal would falsely preserve bank evidence as cleared.
- Existing explicit Payment match and exact auto-match suggestion endpoints now **refuse bank lines linked to a fee journal**. One bank line can no longer be both an operating payment and a fee.
- The read-only bank statement detail returns `feeJournalEntryId` for each line. The bank reconciliation report adds `postedFeeLines` and treats **only a valid Posted System fee journal with matching bank account/date/reference and credit amount** as settled bank evidence. It remains listed as unmatched if fee journal evidence is missing/invalid. `matchedLines` retains its meaning of matched Payments; `postedFeeLines` is separate. `fullyReconciled` still requires signed statement totals = closing balance, no unmatched lines, GL = bank closing and no uncleared posted book payments.

## Tests authored, not executed

Six real authenticated HTTP + migrated PostgreSQL test cases in
`tests/Fintrox.Domain.Tests/Accounting/FinancialHttpPostgresBankFeeTests.cs`:

1. Negative fee: exact two-line posted journal; audit reference, statement details and ledger reconciliation; identical replay with same ID.
2. Refuse wrong amount, Revenue/Bank account instead of Expense, no journal side effects.
3. Refuse positive bank line; reject changed reason on fee replay.
4. Refuse standalone payment-match and any automatic suggestion for a posted fee line.
5. Refuse posting into closed accounting period.
6. Cross-tenant 404; concurrent identical calls make **one** effect and one response indicates replay.

Tests are **written but not confirmed**: local environment has no .NET SDK, and the available commit-status lookup does not provide a passing CI run. Do not report a passing build or migration until measured.

## Remaining tasks

- **F4C foreign-currency bank/FX**: apply explicitly selected booked, transaction and revaluation rates, realized/unrealized differences, gain/loss accounts and currency precision; current bank account binding remains base-currency only.
- **Controlled fee correction/reversal** (separate authorized bank line + journal atomic workflow), fee tax policy/VAT where legally applicable, approval matrix and stronger tenant audit attribution.
- Bank statement chain continuity, bank-specific feed imports (e.g. MT940, ISO 20022 camt), partial/aggregated fees, reconciliation approval/signed evidence, performance work.
- **`ACCOUNTING-001F` and full finance reconciliation remain IN PROGRESS.** This is an operational legacy Banking/Accounting slice; the new multi-project Banking domain Entity scaffolds are not yet wired to the EF runtime.
