# ACCOUNTING-001F4C1 — foreign currency bank settlement and realized FX results

**2026-10-14 — CODE IMPLEMENTED ON main; .NET BUILD, CI, AND MIGRATED POSTGRESQL TESTS NOT YET EXECUTED / VERIFIED.**

This is a bounded first part of ACCOUNTING-001F4C, **not** full foreign-currency bank balance closing or revaluation. It extends the **operational legacy** Banking/Accounting stack. The target 36-module Banking scaffolds are still not EF-mapped.

## API and currency policy

- Existing `POST /api/v1/banking/accounts` now accepts an active organization-configured **foreign** three-letter currency in addition to the organization's base currency. Each bank account still has a **dedicated active analytical Asset GL account**. Its `banking.bank_statements` opening, closing and signed line amounts are native-currency values; the accounting journal GL stays in the **organization's base currency**.
- New `POST /api/v1/banking/statements/{statementId}/lines/{lineId}/post-foreign-settlement` requires BOTH `accounting.write` and `accounting.post`. It is an explicitly approved settlement of a **single positive/negative native-currency bank line** against an explicitly identified existing base-currency AR/AP clearing balance. No arbitrary narration-based matching or silent FX gain assumption.
- Required `PostForeignBankSettlementRequest`: `exchangeRateId`, `clearingAccountId`, `clearingBaseAmount` (positive, max 4 dp), `fxGainAccountId`, `fxLossAccountId`, and `expectedSignedForeignAmount` (signed, nonzero, max 4 dp). The rate must be an existing tenant-owned `ExchangeRate` where `EffectiveDate == statement.valueDate`, `BaseCurrencyId` has the organization's base currency code and `QuoteCurrencyId` is the foreign bank currency, with both currencies active. Rate convention is **foreign quote units per one base unit**: `bankBase = Round(abs(native) / approvedRate, 4, AwayFromZero)`. No guessed or future spot rate. The approved rate is snapshotted immutably at posting time, protecting previous entries if the rate master is edited.
- The counteraccount must be an active tenant analytical `Asset` or `Liability` account, and the FX gain and loss accounts must be active analytical `Revenue` and `Expense` respectively. All four (bank, clearing, gain, loss) must be distinct. The caller is responsible for linking the settlement to the correct invoice/payable or other cleared balance; **the route does not mark an invoice paid or allocate a Payment**.
- **For incoming foreign funds**: DR Bank GL (base value), CR clearing account (specified carrying base), the difference to FX Revenue credit if bank value is greater or FX Expense debit if smaller. **For outgoing funds**: CR Bank GL, DR clearing, realized FX Loss debit if bank value is greater or FX Gain credit if smaller. The new System journal is always balanced. A zero difference creates just two lines.
- Existing GL calendar `FOR SHARE` locks and re-read of year/period statuses forbid posting into closed or misdated periods. Bank account `FOR UPDATE` then bank line `FOR UPDATE` serialize with imports, payment matches and fees. All journal/lines, number allocation and FX settlement evidence commit atomically in one PostgreSQL transaction.
- External reference `bank-fx-settlement:v1:{statementLineGuid:N}` is unique per tenant. Replays return the original posted journal and original approved rate snapshot (`alreadyPosted=true`), including after a later period closure. Different clearing value/accounts/rate, missing source journal, wrong native amount or another bank use returns 409.
- New migration `20261014000300_BankForeignExchangeSettlements`: immutable, unique `banking.bank_fx_settlements` linking tenant statement line, original GL journal, `exchangeRateId`, snapshotted rate, clearing account, bank base value and realized gain/loss + accounts. PostgreSQL tenant composite FKs, check constraints, immutability trigger. The imported bank-native statement lines are **never mutated**.
- Normal base-currency `post-fee`, `match` and deterministic `auto-match` now reject foreign bank accounts/foreign-settled rows, preventing native amounts from being silently treated as base currency. Protected FX system journals cannot be reversed through the generic journal reversal route until a controlled Banking FX correction exists.

## Bank statement read models / non-comparability

- Statement line details now expose `foreignSettlementJournalEntryId`.
- Banking reconciliation reports `postedForeignSettlementLines` separately from matched payments and posted base-currency fee lines. A foreign line counts as processed only if its linked System journal remains Posted, dated correctly, uses the correct dedicated bank GL account **with exact signed booked base value**, and remains balanced. Missing/corrupt GL evidence makes the bank line unmatched.
- **Absolutely no false USD↔EUR balance subtraction.** For a foreign bank account, `bankAndLedgerCurrencyComparable=false`, `statementVsLedgerDifference=null`, `bankAndLedgerBalanced=false` and `fullyReconciled=false` even if every movement was posted. Native statement closing and cumulative GL base closing are displayed as separate amounts, not asserted comparable. For base-currency accounts, existing comparison and conditions remain unchanged.

## Real tests written; none executed

**Six** additional full HTTP + migrated PostgreSQL tests in `tests/Fintrox.Domain.Tests/Accounting/FinancialHttpPostgresForeignBankSettlementTests.cs`:
1. USD +120 @1.2 quoted/USD per EUR, EUR clearing 95 -> DR bank EUR100, CR AR EUR95, CR FX gain EUR5; balance, reference, line evidence, report noncomparability, original-id replay.
2. USD -120 @1.2 and clearing EUR95 -> CR bank EUR100, DR clearing EUR95, DR FX loss EUR5.
3. Reject incorrect original amount, nonexistent/unapproved date rate, conflicting second settlement parameters.
4. Refuse posting foreign-native data via base-currency bank fee/payment/automatic suggestion routes.
5. Foreign tenant cannot access owner statement; closed accounting period rejects new FX posting.
6. Concurrent identical posting requests produce one numbered System journal and one identical replay result.

**These tests have not run.** The available tool/commit status data has not demonstrated a successful .NET build or migrated-PostgreSQL test run. Exact run IDs and errors must be written to `docs/ai/VALIDATION.md` when available.

## Remaining F4C2 / F4C3 and larger project boundaries

- **F4C2: actual unrealized FX revaluation at bank statement/fiscal cutoff.** This requires verified native bank opening/closing balance roll-forward, immutable quote quantity history, independently approved cutoff exchange rate, and the previous base-currency GL carrying amount. Automatically revaluing now from arbitrary unmatched statement lines could misclassify an unposted transaction as a gain/loss, so no such ledger entry is fabricated here.
- **F4C3:** native bank vs GL reconciliations with FX revaluation, exchange-rate source/audit permissions, currency rounding policies, end-to-end payment/document clearing references, realized gains tied to exact subledger allocations, true event-time book reporting.
- FX bank fees, split/aggregated foreign settlements, bank charges/VAT rules, controlled reversal of FX journals, historical reconciliation approvals, ISO20022/MT940 adapters and formal statutory closing remain pending.
- `ACCOUNTING-001F4C` and `ACCOUNTING-001F` are still **IN PROGRESS**. The fiscal `financialSubsystemsFullyReconciled` flag must remain false.

This is financial software: do not use the new FX routes with production financial data until migrations compile, PostgreSQL tests pass, the FX rate/master approval design is reviewed, and auditors validate the close policy.
