# ACCOUNTING-001F2 — AR/AP open-item and control-account reconciliation

**2026-10-14 — CODE IMPLEMENTED; BUILD, CI AND MIGRATED POSTGRESQL TEST EXECUTION NOT VERIFIED.**

This is the second bounded slice of `ACCOUNTING-001F`. It is implemented in the authoritative **operational legacy** Finance/Accounting runtime, **not** the target Accounting Domain scaffolds. It is an internal financial reconciliation report; it never posts, deletes, settles or adjusts GL journals automatically.

## API

`GET /api/v1/accounting/fiscal-years/{fiscalYearId}/open-item-reconciliation`

Requires bearer authorization, active organization and `accounting.read`. Another organization's/nonexistent fiscal year -> 404. The account balances are cumulative from the start of the operational ledger **through `FiscalYear.EndDate`** (not only this fiscal year's movements); older outstanding sales and purchase documents are included.

Response `FinancialOpenItemReconciliationResponse` includes:
- `fiscalYearId`, `cutoffDate`, separate `receivablesOpenBase`, `payablesOpenBase`.
- Document-level `openItems`: `documentId`, `sourceType`, `counterpartyId`, `documentNumber`, document/due dates, original document gross, confirmed payment allocations, open amount in document currency **and** in base currency, historical control `accountId`, `overdue`, `daysOverdue`, `agingBucket` (`Current`, `1-30`, `31-60`, `61-90`, `90+`).
- `counterpartyBalances`: outstanding base-currency AR/AP totals and open/overdue document counts for each customer or supplier counterparty, grouped by role.
- `controlAccounts` with explicit receivable/payable classification, expected sum of subledger open items, posted cumulative GL debit/credit-derived balance, signed difference `actual - expected`, `balanced`.
- `issues` with codes and related document/payment IDs, and scoped `accountsReceivableReconciled`/`accountsPayableReconciled` flags.

**Safety flags:** `historicalAsOfSnapshotAvailable=false`, `bankStatementsReconciled=false`, `financialSubsystemsFullyReconciled=false` regardless of isolated GL balance correctness. These are not full historical or statutory assurances.

## Accounting policy implemented

1. Lock the tenant fiscal-year PostgreSQL row `FOR UPDATE` in an EF transaction; read the current year only afterward. Load tenant-only Sales invoices, Purchase documents, Payment headers and allocations with source/payment date **at or before** `FiscalYear.EndDate`.
2. The current `Issued` invoice or `Received` purchase is a collectible/payable document. Ignore Draft and current Cancelled documents for outstanding exposure; detect allocations to cancelled documents. Draft/Cancelled payments cannot settle outstanding balances.
3. Match allocations by **document GUID**, and require correct direction (incoming -> Sales invoice; outgoing -> Purchase), counterparty match, one target type, positive `DocumentAmount`/`PaymentAmount`, and payment-currency allocated row totals equal to confirmed payment header `AllocatedAmount`. Check cases with no allocation rows, overallocated documents, unmatched target GUIDs and payments exceeding their gross amount.
4. Convert document gross amount and **each document-currency payment allocation separately** into base currency using the original document `ExchangeRate`: `decimal.Round(amount / rate, 4, MidpointRounding.AwayFromZero)`. Net carrying balance = base document gross minus the sum of allocation carrying balances, matching the active `AutoPostingEngine` source settlement policy (and preserving individual rounding). Do not compare payment-currency amounts with document-currency totals or substitute today's FX rate.
5. Obtain actual historical control-account assignment from the original immutable system invoice/purchase GL journal (external source reference + `Accounts receivable -` / `Accounts payable -` control posting line). Include historical accounts and active/inactive mapping rules. Only fall back to a **unique** configured control account where an original line is missing; report missing/duplicate original control lines and incompatible document journal lifecycle.
6. Compare expected open documents **by GL control account** against the cumulative Posted/Reversed accounting journal balance for that same organization through cutoff: AR = Debit - Credit, AP = Credit - Debit. Report discrepancies resulting from manual GL changes, unmatched settlements, migration/opening-balance mismatches, missing original source journals or posting-rule changes. A control account assigned to **both** AR and AP is explicitly rejected for each side.
7. Open amounts (including negative overallocations) are returned transparently; never silently reduce them to zero. Due-date aging uses the requested fiscal-year-end cutoff date.

## Known limitations

- **NOT a legally complete historical aged AR/AP as-of statement.** The year-end date filters document and payment *event dates*, but the legacy source entities expose **current lifecycle status**, not dated status/settlement events. If an invoice was cancelled or a payment reversed after the cutoff, exact historical exposures cannot be reconstructed. The response explicitly advertises `historicalAsOfSnapshotAvailable=false`.
- The report is GL **control-balance based**. It detects missing or incorrect source evidence, but it does not establish legal customer/supplier confirmations, interest accruals, disputed invoices, credit notes, write-offs, debt collection, intercompany netting or account-specific manual journal business justification. Starting balances without corresponding imported legacy open items surface as a difference until opening items are migrated and reconciled.
- The operational `Banking` module is a scaffold. Bank statements, bank transactions, settlement accounts and payment-to-statement matching are **NOT** implemented. Never report enterprise finance fully reconciled.
- Multi-book financial closing, revaluation of open foreign-currency balances, journal dimensional allocations and formal reviewer approval are separate future milestones.
- This API performs a full cumulative read. Large installations require query paging, pre-aggregated accounting read models and explicit as-of event history in later scalability work.

## Real HTTP + migrated PostgreSQL tests authored, not executed

**11** additional cases in the shared disposable PostgreSQL `FinancialHttpPostgresRegressionTests` fixture (file: `tests/Fintrox.Domain.Tests/Accounting/FinancialHttpPostgresOpenItemTests.cs`):

1. AR 100 / payment 40 = AR 60; AP 60 / payment 10 = AP 50, matched GL accounts, invoice IDs, counterparties, overdue classification, no unsupported full-finance claim.
2. Extra manual AR movement of +9 yields exact AR discrepancy and failed AR flag.
3. Customer payment 120 against invoice 100 yields overallocated open item -20 and failed AR flag.
4. Payment on a different counterparty is detected despite otherwise matching GL balances.
5. Extra manual AP movement of +5 yields exact AP discrepancy without falsely failing AR.
6. Missing AR/AP control rules is reported, even when the ledger has no activity.
7. Invoice USD 100 at 1.25 and USD 25 payment allocation yields EUR 60 open, exercising conversion on individual document allocations.
8. Another organization's year returns 404.
9. Full settlement removes both documents from open-item list and returns zero GL differences.
10. Payment-header allocated summary differing from linked payment-currency rows is rejected.
11. A confirmed payment's allocated header value with **no allocation rows** is rejected.

**No .NET compiler or executable PostgreSQL test results are available from authoring these GitHub commits.** CI and integration-test results must be recorded in `docs/ai/VALIDATION.md` before this milestone is called verified.

## Next in ACCOUNTING-001F

- `001F3`: actual Banking runtime, bank statements, import/matching and bank-vs-GL reconciliation. Bank module Domain scaffolds do not substitute for bank transaction persistence.
- Historical event-time AR/AP roll-forward and account-level subledger audit evidence; clear migration of carried-forward prior-year open documents.
- Credit notes, refunds, cash/card clearing, unapplied advances and credit balances, FX revaluation and book-level/organizational dimensions.
- Close approval evidence and end-to-end production/CI validation. `ACCOUNTING-001F` remains IN PROGRESS.
