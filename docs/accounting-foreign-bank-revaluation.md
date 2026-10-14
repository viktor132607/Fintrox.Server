# ACCOUNTING-001F4C2 — guarded unrealized FX revaluation of foreign bank assets

**2026-10-14: IMPLEMENTED IN CODE; .NET COMPILATION, CI, MIGRATED POSTGRESQL TESTS AND PRODUCTION FINANCIAL APPROVAL ARE NOT VERIFIED.**

This is a bounded operational **legacy** Banking/Accounting implementation, not the target 36-module Banking Domain skeleton. It extends realized FX settlement (`ACCOUNTING-001F4C1`); native-bank full close/certification remains `001F4C3`.

## API and formula

`POST /api/v1/banking/statements/{statementId}/revalue-foreign`

Requires tenant membership and BOTH `accounting.write` and `accounting.post`. Input:

```json
{
  "exchangeRateId": "00000000-0000-0000-0000-000000000001",
  "fxGainAccountId": "00000000-0000-0000-0000-000000000002",
  "fxLossAccountId": "00000000-0000-0000-0000-000000000003",
  "expectedNativeClosingBalance": 120.0000,
  "expectedLedgerBaseBalance": 100.0000
}
```

The statement's `to_date` **must exactly match the end of an open accounting period in an open fiscal year**. The bank account must be configured in an active foreign currency, with a dedicated analytical Asset GL account. **Negative statement closing balances / overdrafts are refused** rather than applying Asset signs to a Liability. The operator specifies a tenant-owned ExchangeRate for the **exact cutoff date**, with organization base currency as Base and bank foreign currency as Quote. Fintrox convention: `rate = foreign units per 1 base unit`. The existing ExchangeRate model has **no formal approval-state workflow**: the permission-gated user's choice is explicit, but the rate's institutional approval is not independently evidenced.

Target base-currency carrying value:

```text
TargetBase = Round(NativeClosing / CutoffRate, 4, AwayFromZero)
Adjustment = TargetBase - PostedBankGLBaseBefore
```

Accounting journal (when Adjustment != 0):
- **Positive**: Debit bank Asset / Credit FX gain Revenue.
- **Negative**: Debit FX loss Expense / Credit bank Asset.
- **Zero**: record immutable zero-adjustment valuation evidence **without inventing a zero-amount journal**.

Existing real `JournalEntry` / `JournalLine` / `IJournalRepository` handles two-line balanced System journal, fiscal-year posting number and open-period locking. Reference is `bank-fx-revaluation:v1:{bankAccountGuid:N}:{yyyyMMdd}` (tenant unique). Revaluation history is immutable; generic journal reverse must also be blocked until a bank-specific correction workflow exists.

## Mandatory evidence gates before posting

Inside one PostgreSQL transaction:
1. Tenant-scoped source statement and account lookup; lock bank account `FOR UPDATE` before checking replay, later lock fiscal-year+period via existing GL calendar posting locks. Only the cutoff statement can be revalued.
2. Identical request replay returns **original FX rate, original bank GL carrying amount, adjustment and original journal ID** after future period close. Changed rate, gain/loss accounts, expected native/base amounts or a reversed/changed old journal -> 409. Unique bank-account/cutoff and statement keys reject racing duplicate valuations.
3. Verify imported statement history through cutoff is complete and contiguous (date range with no gaps; prior closing must equal next opening). The first available imported statement must start with a **zero native opening**. Reject any prior posted bank GL movements preceding this first statement. These conservative prerequisites deliberately exclude legacy accounts whose opening balances have not been independently migrated and reconciled.
4. Verify **every imported native line** up to cutoff has exactly one recognized **Posted** FX settlement source journal on the correct bank Asset account, matching value date and signed base amount. Refuse unmatched payments, fees or other unknown bank movements. Verify each statement's opening + sum(signed lines) == closing.
5. Check that **every posted bank GL movement** through cutoff is traceable to a settled foreign statement line or an earlier immutable FX revaluation for that exact bank account. Unrecognized manual/base-currency journals or Reversed bank journals prevent revaluation.
6. Compare the independently computed cumulative posted bank GL balance with the user's `expectedLedgerBaseBalance` before the adjustment. If changed, return 409 rather than posting against stale numbers.
7. Require two distinct, active, analytical and tenant-owned gain/loss accounts of respective Revenue and Expense types. Reject zero currency rates, wrong date/pair, missing rate, closed accounting periods or foreign organization.
8. Atomically write the posted balanced System journal (if nonzero) **and** immutable valuation evidence in the same transaction. Zero-delta valuations commit only audit evidence. All temporary changes roll back together on error.

## Schema and overlap guards

`20261014000400_BankForeignExchangeRevaluations.cs` adds `banking.bank_fx_revaluations` with tenant-composite foreign keys, unique bank account/cutoff and statement, optional journal only for zero delta, approved selected rate ID plus **immutable 10-decimal snapshot**, signed native closing, GL base before, base target, gain/loss mappings, adjustment and creation timestamp; trigger blocks UPDATE/DELETE.

- `ACCOUNTING-001F4C1` now rejects **new backdated FX settlements** on or before an already-valued cutoff.
- Existing bank-statement import now rejects **backdated statements** starting on or before a valued cutoff. Previously imported exact-replay continues through the read-only replay branch.
- No silent retrospective restatement or journal reversal is allowed. Corrections require a separate approved, atomic correction process.

## Authored tests, NOT executed

**9 actual authenticated HTTP + migrated PostgreSQL integration tests**, `tests/Fintrox.Domain.Tests/Accounting/FinancialHttpPostgresBankRevaluationTests.cs`:
- EUR bank GL 100, native USD 120: cutoff 1 USD per EUR -> DR Bank EUR 20 / CR FX Gain EUR 20, balanced and replay-safe.
- Cutoff 1.5 USD per EUR -> DR FX Loss EUR 20 / CR Bank EUR 20.
- Cutoff 1.2 -> zero delta, no dummy journal and one immutable zero valuation record.
- Reject unmatched/unsettled foreign statement lines.
- Reject wrong date/nonexistent rate or stale expected bank GL balance.
- Reject another organization and closed period.
- Reject backdated import after immutable cutoff.
- Reject negative-native-bank overdraft under Asset accounting.
- Concurrent duplicate requests produce one GL journal and one replay result.

These are **written, not passing-test claims**. A real .NET/EF compilation and migrated PostgreSQL test run must follow; report any errors in `docs/ai/VALIDATION.md`.

## Explicit limitations and next

1. **Not a complete foreign bank reconciliation**: native closing and base GL carrying value are bridged at cutoff, but the older banking report deliberately retains `bankAndLedgerCurrencyComparable=false` and `fullyReconciled=false` for foreign accounts until `ACCOUNTING-001F4C3` implements a native quantity ↔ GL base bridge and signed close checklist.
2. The system does not currently provide separately reviewed ExchangeRate approvals, settlement-to-invoice subledger allocations, external bank statement authenticity, multicurrency bank fees, card/merchant clearing or FX transaction lot costing.
3. The bank may only be valued here if it has a trusted, uninterrupted **zero-opening** statement history and uniquely sourced GL movements. Migrated pre-existing bank balances, nonzero opening bank statements, overdrafts and historic untraceable journals are deliberately blocked until special migration and liability policies exist.
4. Generic/manual GL source changes can still occur through other accounting paths while the period is open; full period closing, four-eyes sign-off, historic correctness and integration tests remain required before the calculation is a certified enterprise financial close.
5. **F4C3**: native-value reconciliation read model, cross-period valuation bridge, four-eyes approval and controlled corrections for valued statements. `ACCOUNTING-001F` remains **IN PROGRESS** and the global `financialSubsystemsFullyReconciled` flag must stay false.
