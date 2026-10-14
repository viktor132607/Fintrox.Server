# ACCOUNTING-001F4C3A — foreign native/base bank close evidence and maker/checker review

**2026-10-14: CODE IMPLEMENTED ON main — .NET BUILD, MIGRATIONS, REAL POSTGRESQL TESTS AND CI NOT VERIFIED.**

A **bounded internal foreign bank reconciliation** has been added to the operational legacy Accounting/Banking API. This is **not** a legally signed statutory bank reconciliation, nor a correction/reversal workflow. The new 36-module Banking scaffolds remain unmapped.

## Three authenticated routes

- `GET /api/v1/banking/statements/{statementId}/foreign-close-evidence` — `accounting.read`.
- `POST /api/v1/banking/statements/{statementId}/prepare-foreign-close` — `accounting.write` **and** `accounting.post`; body `{ "reviewNote": "At least ten characters ..." }`.
- `POST /api/v1/banking/statements/{statementId}/approve-foreign-close` — `accounting.post` **and** `organizations.manage`; body `{ "approvalNote": "Independent verification ..." }`. The reviewer **must be a different authenticated user** from the preparer, not a second session of the same identity.

Missing or foreign-tenant statement -> 404; missing permissions -> 401/403; incomplete source evidence, conflicting replay, self-approval, or changed evidence -> 409.

## Native currency ↔ valued GL bridge

The read model explicitly reconciles foreign bank statement closing **in native units** to a base-currency GL carrying value via its already booked and immutable period-end FX revaluation record:

```text
ExpectedBankBase = Round(NativeClosing / StoredCutoffRate, 4, AwayFromZero)
GL Difference = PostedLedgerBaseThroughCutoff - ExpectedBankBase
```

It checks the complete zero-opening native statement chain, contiguous statement periods and arithmetic, all native movements linked to valid Posted System FX settlement journals on the bank's dedicated asset GL account with the same date/reference/booked base value and balanced GL lines, no unknown/Reversed bank GL movements, a matching immutable cutoff FX revaluation with its own verified Posted System journal (two balanced lines, correct bank adjustment, expected journal reference), and zero expected/actual base difference. The evidence report returns explicit per-check booleans, original exchange rate and balances, issue codes, and `internallyReconciled`.

Unlike the old generic bank report, this **does not directly subtract native bank units from GL base units**. It uses the original recorded conversion snapshot to reconcile the two. Existing generic fiscal `financialSubsystemsFullyReconciled` remains false, and `fullyReconciled` in the older generic foreign bank report remains false until additional formal review/controls are completed.

## Independent immutable maker/checker evidence

`20261014000500_ForeignBankCloseApprovals` adds `banking.bank_fx_close_preparations` and `banking.bank_fx_close_approvals`, with tenant-composite statement/account/preparation foreign keys, one preparation per statement and one approval per preparation, maker/reviewer IDs, review notes, SHA-256 checksum of stable reconciliation evidence and database immutability triggers.

At **prepare**, the service acquires the tenant's bank-account `FOR UPDATE` lock followed by **exclusive** fiscal-year then accounting-period `FOR UPDATE` locks, which block concurrent standard journal posting (`FOR SHARE`) while approval evidence is computed; it then recomputes the native/base bridge and rejects incomplete/misbalanced evidence. Stores who prepared, the statement cutoff and balances, plus the evidence fingerprint. Identical replay is safe; changed preparer/note/evidence conflicts.

At **approve**, it again locks and recomputes the report, requires all internal validations still pass, verifies the originally prepared SHA-256 fingerprint, requires another authenticated user, and then atomically writes separate immutable approval evidence. Exact reviewer/note replay succeeds; another reviewer or changed terms conflicts. Approval does **not** post any GL journals or change imported bank statement evidence.

The API reports both `prepared` and `approved` only when the fingerprint still equals current recalculated evidence. New unexpected GL postings can make a previously approved internal confirmation appear stale; it is not silently carried forward as valid.

**Important:** The fingerprint is an application-built checksum of selected immutable statement, revaluation and GL balance/issue fields. It is NOT a bank digital signature, qualified electronic signature, external tamper-proof notary, or permanent legal close. Automated close sign-off and full finance completeness cannot be asserted from this alone.

## Five new HTTP + migrated PostgreSQL tests written; not run

`tests/Fintrox.Domain.Tests/Accounting/FinancialHttpPostgresBankForeignCloseTests.cs`:

1. USD native closing 120 and stored rate 1 produce expected EUR 120 and zero post-revaluation difference, with validated source/journal evidence.
2. Maker prepares, cannot approve their own close; a distinct authorized organization member can approve; identical approval replay returns original.
3. Cannot prepare a close without completed immutable cutoff revaluation.
4. A later unauthorized manual bank GL entry invalidates the prepared fingerprint and blocks checker approval.
5. The other tenant cannot read, prepare or approve the bank close.

**All five are authored, not confirmed passing.** Current tool access has not produced a .NET compilation or migrated PostgreSQL run.

## Remaining ACCOUNTING-001F4C3B / 001F

- **Controlled journal/source correction** for approved/revalued fee and FX bank journal records is **NOT IMPLEMENTED**. It requires an atomic correction workflow that compensates the GL, records new immutable source evidence, invalidates/versions the review and handles closed fiscal windows. Generic reversal remains blocked for protected financial source journals. Never edit imported bank lines or delete approved review history.
- Full native foreign bank legacy opening-balance migration, foreign overdraft policy, irregular statement coverage, external bank statement authenticity, independent exchange-rate master approval and true four-eyes statutory close attestation remain pending.
- Approval exists only for foreign-bank cutoff evidence, not for the whole tenant ledger, AR/AP, tax, inventory, payroll or consolidated enterprise finance. The main `ACCOUNTING-001` task remains **IN PROGRESS**.
