using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Fintrox.Contracts.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace Fintrox.Infrastructure.Accounting;

/// <summary>
/// Read-only native/base GL valuation bridge and immutable two-person
/// internal reconciliation approval. Approval never changes booked journals.
/// </summary>
public sealed partial class BankReconciliationService
{
    private sealed record ForeignCloseSnapshot(
        ForeignBankCloseEvidenceResponse Report, string Fingerprint,
        Guid? AmendmentId);

    public async Task<ForeignBankCloseEvidenceResponse?> GetForeignCloseEvidenceAsync(
        Guid statementId, CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(
                IsolationLevel.RepeatableRead, cancellationToken);
            var cn = (NpgsqlConnection)db.Database.GetDbConnection();
            var snapshot = await ReadForeignCloseAsync(
                cn, tx, tenant, statementId, cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return snapshot?.Report;
        });
    }

    public async Task<ForeignBankCloseDecisionResponse?> PrepareForeignCloseAsync(
        Guid statementId, PrepareForeignBankCloseRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        var maker = currentUser.RequireUserId();
        var note = request.ReviewNote?.Trim();
        if (string.IsNullOrWhiteSpace(note) || note.Length is < 10 or > 500)
            throw new BankReconciliationConflictException(
                "A 10-500 character bank reconciliation review note is required.");

        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
            var cn = (NpgsqlConnection)db.Database.GetDbConnection();
            var accountId = await LockBankAccountForCloseAsync(
                cn, tx, tenant, statementId, cancellationToken);
            if (accountId is null) return null;

            await LockCloseCalendarAsync(
                cn, tx, tenant, statementId, cancellationToken);
            var evidence = await ReadForeignCloseAsync(
                cn, tx, tenant, statementId, cancellationToken);
            if (evidence is null) return null;
            if (!evidence.Report.InternallyReconciled)
                throw new BankReconciliationConflictException(
                    "Bank FX cutoff is not internally reconciled: " +
                    string.Join(", ", evidence.Report.Issues));

            Guid preparationId;
            Guid preparedBy;
            string previousHash;
            var preparationLookupSql = evidence.AmendmentId.HasValue
                ? """
                  SELECT id,prepared_by_user_id,evidence_sha256,review_note
                  FROM banking.bank_fx_amendment_preparations
                  WHERE organization_id=@org AND amendment_id=@amendment
                  """
                : """
                  SELECT id,prepared_by_user_id,evidence_sha256,review_note
                  FROM banking.bank_fx_close_preparations
                  WHERE organization_id=@org AND statement_id=@statement
                  """;
            await using (var previous = Command(cn, tx, preparationLookupSql,
                ("org", tenant), ("statement", statementId),
                ("amendment", (object?)evidence.AmendmentId ?? DBNull.Value)))
            await using (var reader = await previous.ExecuteReaderAsync(cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                {
                    preparationId = reader.GetGuid(0);
                    preparedBy = reader.GetGuid(1);
                    previousHash = reader.GetString(2);
                    if (preparedBy != maker || previousHash != evidence.Fingerprint ||
                        reader.GetString(3) != note)
                        throw new BankReconciliationConflictException(
                            "This bank cutoff is already prepared by a different maker or with different evidence.");
                    return new ForeignBankCloseDecisionResponse(
                        statementId, accountId.Value, preparationId,
                        preparedBy, evidence.Report.ApprovedByUserId,
                        previousHash, evidence.Report.Approved,
                        AlreadyRecorded: true,
                        VersionNumber: evidence.Report.VersionNumber);
                }
            }

            preparationId = Guid.NewGuid();
            var preparationInsertSql = evidence.AmendmentId.HasValue
                ? """
                  INSERT INTO banking.bank_fx_amendment_preparations (
                      id,organization_id,amendment_id,
                      prepared_by_user_id,evidence_sha256,review_note)
                  VALUES (@id,@org,@amendment,@maker,@hash,@note)
                  """
                : """
                  INSERT INTO banking.bank_fx_close_preparations (
                      id,organization_id,statement_id,bank_account_id,cutoff_date,
                      prepared_by_user_id,evidence_sha256,native_closing_balance,
                      expected_ledger_base,review_note)
                  VALUES (@id,@org,@statement,@account,@cutoff,@maker,@hash,@native,@base,@note)
                  """;
            await using (var insert = Command(cn, tx, preparationInsertSql,
                ("amendment", (object?)evidence.AmendmentId ?? DBNull.Value),
                ("id", preparationId), ("org", tenant),
                ("statement", statementId), ("account", accountId.Value),
                ("cutoff", evidence.Report.CutoffDate), ("maker", maker),
                ("hash", evidence.Fingerprint),
                ("native", evidence.Report.NativeClosingBalance),
                ("base", evidence.Report.ActualPostedLedgerBase),
                ("note", note!)))
                await insert.ExecuteNonQueryAsync(cancellationToken);

            await tx.CommitAsync(cancellationToken);
            return new ForeignBankCloseDecisionResponse(
                statementId, accountId.Value, preparationId, maker,
                null, evidence.Fingerprint, Approved: false,
                AlreadyRecorded: false,
                VersionNumber: evidence.Report.VersionNumber);
        });
    }

    public async Task<ForeignBankCloseDecisionResponse?> ApproveForeignCloseAsync(
        Guid statementId, ApproveForeignBankCloseRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        var checker = currentUser.RequireUserId();
        var note = request.ApprovalNote?.Trim();
        if (string.IsNullOrWhiteSpace(note) || note.Length is < 10 or > 500)
            throw new BankReconciliationConflictException(
                "A 10-500 character independent approval note is required.");

        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
            var cn = (NpgsqlConnection)db.Database.GetDbConnection();
            var accountId = await LockBankAccountForCloseAsync(
                cn, tx, tenant, statementId, cancellationToken);
            if (accountId is null) return null;

            await LockCloseCalendarAsync(
                cn, tx, tenant, statementId, cancellationToken);
            var evidence = await ReadForeignCloseAsync(
                cn, tx, tenant, statementId, cancellationToken);
            if (evidence is null) return null;
            if (!evidence.Report.InternallyReconciled)
                throw new BankReconciliationConflictException(
                    "Bank FX evidence has become unbalanced; approval refused.");

            Guid preparationId;
            Guid maker;
            string preparedHash;
            var approvedVersionPreparationSql = evidence.AmendmentId.HasValue
                ? """
                  SELECT id,prepared_by_user_id,evidence_sha256
                  FROM banking.bank_fx_amendment_preparations
                  WHERE organization_id=@org AND amendment_id=@amendment
                  """
                : """
                  SELECT id,prepared_by_user_id,evidence_sha256
                  FROM banking.bank_fx_close_preparations
                  WHERE organization_id=@org AND statement_id=@statement
                  """;
            await using (var previous = Command(cn, tx, approvedVersionPreparationSql,
                ("org", tenant), ("statement", statementId),
                ("amendment", (object?)evidence.AmendmentId ?? DBNull.Value)))
            await using (var reader = await previous.ExecuteReaderAsync(cancellationToken))
            {
                if (!await reader.ReadAsync(cancellationToken))
                    throw new BankReconciliationConflictException(
                        "Another authenticated accountant must prepare the FX bank reconciliation first.");
                preparationId = reader.GetGuid(0);
                maker = reader.GetGuid(1);
                preparedHash = reader.GetString(2);
            }
            if (maker == checker)
                throw new BankReconciliationConflictException(
                    "Maker/checker separation requires two different authenticated users.");
            if (preparedHash != evidence.Fingerprint)
                throw new BankReconciliationConflictException(
                    "The bank/native/GL source evidence changed since the maker prepared this cutoff.");

            var checkerApprovalLookupSql = evidence.AmendmentId.HasValue
                ? """
                  SELECT approved_by_user_id,evidence_sha256,approval_note
                  FROM banking.bank_fx_amendment_approvals
                  WHERE organization_id=@org AND preparation_id=@prep
                  """
                : """
                  SELECT approved_by_user_id,evidence_sha256,approval_note
                  FROM banking.bank_fx_close_approvals
                  WHERE organization_id=@org AND preparation_id=@prep
                  """;
            await using (var oldApproval = Command(cn, tx, checkerApprovalLookupSql,
                ("org", tenant), ("prep", preparationId)))
            await using (var reader = await oldApproval.ExecuteReaderAsync(cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                {
                    if (reader.GetGuid(0) != checker ||
                        reader.GetString(1) != evidence.Fingerprint ||
                        reader.GetString(2) != note)
                        throw new BankReconciliationConflictException(
                            "This bank cutoff has already been independently approved.");
                    return new ForeignBankCloseDecisionResponse(
                        statementId, accountId.Value, preparationId, maker,
                        checker, evidence.Fingerprint, Approved: true,
                        AlreadyRecorded: true,
                        VersionNumber: evidence.Report.VersionNumber);
                }
            }

            var checkerApprovalInsertSql = evidence.AmendmentId.HasValue
                ? """
                  INSERT INTO banking.bank_fx_amendment_approvals
                      (id,organization_id,preparation_id,approved_by_user_id,
                       evidence_sha256,approval_note)
                  VALUES (@id,@org,@prep,@checker,@hash,@note)
                  """
                : """
                  INSERT INTO banking.bank_fx_close_approvals
                      (id,organization_id,preparation_id,approved_by_user_id,
                       evidence_sha256,approval_note)
                  VALUES (@id,@org,@prep,@checker,@hash,@note)
                  """;
            await using (var insert = Command(cn, tx, checkerApprovalInsertSql,
                ("id", Guid.NewGuid()), ("org", tenant),
                ("prep", preparationId), ("checker", checker),
                ("hash", evidence.Fingerprint), ("note", note!)))
                await insert.ExecuteNonQueryAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return new ForeignBankCloseDecisionResponse(
                statementId, accountId.Value, preparationId, maker,
                checker, evidence.Fingerprint, Approved: true,
                AlreadyRecorded: false,
                VersionNumber: evidence.Report.VersionNumber);
        });
    }

    private async Task<Guid?> LockBankAccountForCloseAsync(
        NpgsqlConnection cn, IDbContextTransaction tx, Guid tenant,
        Guid statementId, CancellationToken ct)
    {
        Guid accountId;
        await using (var statement = Command(cn, tx, """
            SELECT bank_account_id FROM banking.bank_statements
            WHERE id=@statement AND organization_id=@org
            """, ("statement", statementId), ("org", tenant)))
        {
            if (await statement.ExecuteScalarAsync(ct) is not Guid id)
                return null;
            accountId = id;
        }
        await using (var account = Command(cn, tx, """
            SELECT id FROM banking.bank_accounts
            WHERE id=@account AND organization_id=@org FOR UPDATE
            """, ("account", accountId), ("org", tenant)))
            await account.ExecuteScalarAsync(ct);
        return accountId;
    }

    private async Task LockCloseCalendarAsync(
        NpgsqlConnection cn, IDbContextTransaction tx,
        Guid tenant, Guid statementId, CancellationToken ct)
    {
        DateOnly toDate;
        await using (var statement = Command(cn, tx, """
            SELECT to_date FROM banking.bank_statements
            WHERE organization_id=@org AND id=@statement
            """, ("org", tenant), ("statement", statementId)))
            toDate = (DateOnly)(await statement.ExecuteScalarAsync(ct))!;

        var period = await calendars.FindPeriodByDateAsync(
            tenant, toDate, trackChanges: false, ct);
        if (period is null || period.EndDate != toDate)
            throw new BankReconciliationConflictException(
                "FX bank closing requires an exact accounting period-end date.");
        // The review requires EXCLUSIVE calendar locks: ordinary journal
        // posting uses FOR SHARE, so another posting cannot change the bank
        // GL between fingerprint validation and committing approval.
        await using (var yearLock = Command(cn, tx, """
            SELECT id FROM accounting.fiscal_years
            WHERE organization_id=@org AND id=@year FOR UPDATE
            """, ("org", tenant), ("year", period.FiscalYearId)))
            if (await yearLock.ExecuteScalarAsync(ct) is not Guid)
                throw new BankReconciliationConflictException(
                    "Fiscal year no longer exists for bank review.");

        await using (var periodLock = Command(cn, tx, """
            SELECT id FROM accounting.accounting_periods
            WHERE organization_id=@org AND fiscal_year_id=@year
              AND id=@period FOR UPDATE
            """, ("org", tenant), ("year", period.FiscalYearId),
            ("period", period.Id)))
            if (await periodLock.ExecuteScalarAsync(ct) is not Guid)
                throw new BankReconciliationConflictException(
                    "Fiscal period no longer exists for bank review.");
    }

    private async Task<ForeignCloseSnapshot?> ReadForeignCloseAsync(
        NpgsqlConnection cn, IDbContextTransaction tx, Guid tenant,
        Guid statementId, CancellationToken ct)
    {
        Guid account;
        Guid bankGl;
        DateOnly cutoff;
        string currency, baseCurrency, statementHash;
        decimal native;
        Guid? rateId = null, valuationJournal = null;
        Guid? gainAccount = null, lossAccount = null;
        decimal rate = 0m, target = 0m, previousGl = 0m, adjustment = 0m;
        await using (var cmd = Command(cn, tx, """
            SELECT a.id,a.gl_account_id,a.currency_code,o.base_currency_code,
                s.to_date,s.closing_balance,s.payload_sha256,
                rv.exchange_rate_id,rv.approved_rate,
                rv.target_base_balance,rv.ledger_base_before,
                rv.adjustment_base,rv.journal_entry_id,
                rv.gain_account_id,rv.loss_account_id
            FROM banking.bank_statements s
            JOIN banking.bank_accounts a
              ON a.id=s.bank_account_id AND a.organization_id=s.organization_id
            JOIN core.organizations o ON o.id=s.organization_id
            LEFT JOIN banking.bank_fx_revaluations rv
              ON rv.statement_id=s.id AND rv.organization_id=s.organization_id
            WHERE s.id=@statement AND s.organization_id=@org
            """, ("statement", statementId), ("org", tenant)))
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            if (!await reader.ReadAsync(ct)) return null;
            account = reader.GetGuid(0);
            bankGl = reader.GetGuid(1);
            currency = reader.GetString(2);
            baseCurrency = reader.GetString(3);
            cutoff = reader.GetFieldValue<DateOnly>(4);
            native = reader.GetDecimal(5);
            statementHash = reader.GetString(6);
            if (!reader.IsDBNull(7))
            {
                rateId = reader.GetGuid(7);
                rate = reader.GetDecimal(8);
                target = reader.GetDecimal(9);
                previousGl = reader.GetDecimal(10);
                adjustment = reader.GetDecimal(11);
                valuationJournal = reader.IsDBNull(12) ? null : reader.GetGuid(12);
                gainAccount = reader.GetGuid(13);
                lossAccount = reader.GetGuid(14);
            }
        }

        // The original cutoff row is immutable. A restatement creates an
        // append-only amendment; current financial valuation is always its
        // highest version. Old maker/checker records remain historical only.
        var version = 1;
        Guid? amendmentId = null;
        await using (var amended = Command(cn, tx, """
            SELECT id,version_number,exchange_rate_id,approved_rate,
                   replacement_target_base,ledger_base_before,
                   replacement_adjustment_base,replacement_journal_id,
                   gain_account_id,loss_account_id
            FROM banking.bank_fx_revaluation_amendments
            WHERE organization_id=@org AND statement_id=@statement
            ORDER BY version_number DESC LIMIT 1
            """, ("org", tenant), ("statement", statementId)))
        await using (var reader = await amended.ExecuteReaderAsync(ct))
        {
            if (await reader.ReadAsync(ct))
            {
                amendmentId = reader.GetGuid(0);
                version = reader.GetInt32(1);
                rateId = reader.GetGuid(2);
                rate = reader.GetDecimal(3);
                target = reader.GetDecimal(4);
                previousGl = reader.GetDecimal(5);
                adjustment = reader.GetDecimal(6);
                valuationJournal = reader.IsDBNull(7) ? null : reader.GetGuid(7);
                gainAccount = reader.GetGuid(8);
                lossAccount = reader.GetGuid(9);
            }
        }

        var issues = new List<string>();
        if (currency == baseCurrency) issues.Add("BankNotForeignCurrency");
        if (rateId is null || rate <= 0m) issues.Add("MissingFxCutoffRevaluation");
        if (native < 0m) issues.Add("ForeignOverdraftRequiresLiabilityPolicy");
        if (rateId is not null && decimal.Round(native / rate, 4,
            MidpointRounding.AwayFromZero) != target)
            issues.Add("CutoffRateTargetMismatch");

        var chain = new List<(Guid Id, DateOnly Start, DateOnly End,
            decimal Opening, decimal Closing, decimal LineTotal)>();
        await using (var cmd = Command(cn, tx, """
            SELECT s.id,s.from_date,s.to_date,
                s.opening_balance,s.closing_balance,
                COALESCE((
                    SELECT SUM(l.signed_amount)
                    FROM banking.bank_statement_lines l
                    WHERE l.statement_id=s.id AND l.organization_id=s.organization_id
                ),0)
            FROM banking.bank_statements s
            WHERE s.organization_id=@org AND s.bank_account_id=@account
              AND s.from_date<=@cutoff
            ORDER BY s.from_date,s.id
            """, ("org", tenant), ("account", account), ("cutoff", cutoff)))
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
                chain.Add((reader.GetGuid(0), reader.GetFieldValue<DateOnly>(1),
                    reader.GetFieldValue<DateOnly>(2), reader.GetDecimal(3),
                    reader.GetDecimal(4), reader.GetDecimal(5)));
        }
        var chainValid = chain.Count > 0 && chain[^1].Id == statementId &&
            chain[^1].End == cutoff && chain[0].Opening == 0m;
        for (var i = 0; i < chain.Count; i++)
        {
            if (chain[i].Start > chain[i].End ||
                chain[i].Opening + chain[i].LineTotal != chain[i].Closing ||
                (i > 0 && (chain[i].Start != chain[i - 1].End.AddDays(1) ||
                            chain[i].Opening != chain[i - 1].Closing)))
                chainValid = false;
        }
        if (!chainValid) issues.Add("IncompleteNativeBankStatementChain");

        long invalidLines;
        await using (var cmd = Command(cn, tx, """
            SELECT count(*)
            FROM banking.bank_statement_lines l
            JOIN banking.bank_statements s
              ON s.id=l.statement_id AND s.organization_id=l.organization_id
            LEFT JOIN banking.bank_fx_settlements fx
              ON fx.statement_line_id=l.id AND fx.organization_id=l.organization_id
            LEFT JOIN accounting.journal_entries e
              ON e.id=fx.journal_entry_id AND e.organization_id=fx.organization_id
            WHERE l.organization_id=@org AND s.bank_account_id=@account
              AND s.to_date<=@cutoff
              AND (l.matched_payment_id IS NOT NULL OR fx.id IS NULL
                OR e.status IS DISTINCT FROM 'Posted'
                OR e.source IS DISTINCT FROM 'System'
                OR e.posting_date IS DISTINCT FROM l.value_date
                OR e.external_reference IS DISTINCT FROM
                    'bank-fx-settlement:v1:' || replace(l.id::text,'-','')
                OR (SELECT count(*) FROM accounting.journal_lines j
                    WHERE j.journal_entry_id=e.id AND j.organization_id=e.organization_id
                      AND j.account_id=@ledger
                      AND j.debit-j.credit = CASE WHEN l.signed_amount>0
                          THEN fx.bank_base_amount ELSE -fx.bank_base_amount END)<>1
                OR (SELECT COALESCE(SUM(j.debit-j.credit),0)
                    FROM accounting.journal_lines j
                    WHERE j.journal_entry_id=e.id AND j.organization_id=e.organization_id)<>0
                OR EXISTS (SELECT 1 FROM banking.bank_fee_postings fee
                    WHERE fee.statement_line_id=l.id AND fee.organization_id=l.organization_id))
            """, ("org", tenant), ("account", account),
            ("cutoff", cutoff), ("ledger", bankGl)))
            invalidLines = Convert.ToInt64(await cmd.ExecuteScalarAsync(ct));
        if (invalidLines != 0) issues.Add("UnpostedOrIncorrectNativeSettlement");

        long unknownJournalLines;
        await using (var cmd = Command(cn, tx, """
            SELECT count(*) FROM accounting.journal_lines j
            JOIN accounting.journal_entries e
              ON e.id=j.journal_entry_id AND e.organization_id=j.organization_id
            WHERE j.organization_id=@org AND j.account_id=@ledger
              AND e.posting_date<=@cutoff
              AND e.status IN ('Posted','Reversed')
              AND NOT (
                (e.status='Posted' AND EXISTS (
                    SELECT 1 FROM banking.bank_fx_settlements fx
                    JOIN banking.bank_statement_lines l
                      ON l.id=fx.statement_line_id AND l.organization_id=fx.organization_id
                    JOIN banking.bank_statements s
                      ON s.id=l.statement_id AND s.organization_id=l.organization_id
                    WHERE fx.organization_id=@org AND fx.journal_entry_id=e.id
                      AND s.bank_account_id=@account))
                OR EXISTS (
                    SELECT 1 FROM banking.bank_fx_revaluations rv
                    WHERE rv.organization_id=@org
                      AND rv.journal_entry_id=e.id AND rv.bank_account_id=@account
                      AND (e.status='Posted' OR
                           (e.status='Reversed' AND EXISTS(
                               SELECT 1 FROM banking.bank_fx_revaluation_amendments am
                               WHERE am.organization_id=@org
                                 AND am.previous_journal_id=e.id
                                 AND am.bank_account_id=@account))))
                OR EXISTS (
                    SELECT 1 FROM banking.bank_fx_revaluation_amendments am
                    WHERE am.organization_id=@org AND am.bank_account_id=@account
                      AND (
                         (am.replacement_journal_id=e.id AND
                            (e.status='Posted' OR
                             (e.status='Reversed' AND EXISTS (
                                 SELECT 1 FROM banking.bank_fx_revaluation_amendments nxt
                                 WHERE nxt.organization_id=@org
                                   AND nxt.previous_journal_id=e.id
                                   AND nxt.bank_account_id=@account))))
                         OR (am.reversal_journal_id=e.id AND e.status='Posted')
                      ))
              )
            """, ("org", tenant), ("ledger", bankGl),
            ("cutoff", cutoff), ("account", account)))
            unknownJournalLines = Convert.ToInt64(await cmd.ExecuteScalarAsync(ct));
        if (unknownJournalLines != 0) issues.Add("UnknownOrReversedBankGlEntry");

        decimal actualGl;
        await using (var cmd = Command(cn, tx, """
            SELECT COALESCE(SUM(j.debit-j.credit),0)
            FROM accounting.journal_lines j
            JOIN accounting.journal_entries e
              ON e.id=j.journal_entry_id AND e.organization_id=j.organization_id
            WHERE j.organization_id=@org AND j.account_id=@ledger
              AND e.posting_date<=@cutoff
              AND e.status IN ('Posted','Reversed')
            """, ("org", tenant), ("ledger", bankGl), ("cutoff", cutoff)))
            actualGl = (decimal)(await cmd.ExecuteScalarAsync(ct))!;

        // Every predecessor valuation must have a full compensating GL
        // journal and a linked Reversed state. A missing amendment journal
        // invalidates all subsequent reviews, even if the net GL accidentally
        // equals the expected target.
        long invalidAmendments;
        await using (var audit = Command(cn, tx, """
            SELECT count(*)
            FROM banking.bank_fx_revaluation_amendments am
            LEFT JOIN accounting.journal_entries old
              ON old.id=am.previous_journal_id AND old.organization_id=am.organization_id
            LEFT JOIN accounting.journal_entries inverse
              ON inverse.id=am.reversal_journal_id AND inverse.organization_id=am.organization_id
            LEFT JOIN accounting.journal_entries replacement
              ON replacement.id=am.replacement_journal_id
             AND replacement.organization_id=am.organization_id
            WHERE am.organization_id=@org AND am.statement_id=@statement
              AND (
                (am.previous_adjustment_base<>0 AND (
                    old.status IS DISTINCT FROM 'Reversed'
                    OR old.reversed_by_journal_entry_id IS DISTINCT FROM inverse.id
                    OR inverse.status IS DISTINCT FROM 'Posted'
                    OR inverse.reversal_of_journal_entry_id IS DISTINCT FROM old.id
                    OR (SELECT count(*) FROM accounting.journal_lines j
                        WHERE j.organization_id=@org AND j.journal_entry_id=old.id
                          AND j.account_id=@ledger
                          AND j.debit-j.credit=am.previous_adjustment_base)<>1
                    OR (SELECT count(*) FROM accounting.journal_lines j
                        WHERE j.organization_id=@org AND j.journal_entry_id=inverse.id
                          AND j.account_id=@ledger
                          AND j.debit-j.credit=-am.previous_adjustment_base)<>1
                ))
                OR (am.replacement_adjustment_base<>0 AND (
                    replacement.status IS DISTINCT FROM 'Posted'
                    AND NOT EXISTS (
                        SELECT 1 FROM banking.bank_fx_revaluation_amendments nxt
                        WHERE nxt.organization_id=@org
                          AND nxt.previous_journal_id=replacement.id
                          AND replacement.status='Reversed')
                    OR (SELECT count(*) FROM accounting.journal_lines j
                        WHERE j.organization_id=@org
                          AND j.journal_entry_id=replacement.id
                          AND j.account_id=@ledger
                          AND j.debit-j.credit=am.replacement_adjustment_base)<>1
                ))
              )
            """, ("org", tenant), ("statement", statementId),
            ("ledger", bankGl)))
            invalidAmendments = Convert.ToInt64(
                await audit.ExecuteScalarAsync(ct));
        if (invalidAmendments != 0)
            issues.Add("BrokenVersionedFxCorrectionEvidence");

        bool validValuation = rateId is not null &&
            previousGl + adjustment == target;
        if (validValuation && valuationJournal.HasValue)
        {
            await using var cmd = Command(cn, tx, """
                SELECT count(*)=1 FROM accounting.journal_entries e
                WHERE e.id=@journal AND e.organization_id=@org
                  AND e.status='Posted' AND e.source='System'
                  AND e.posting_date=@cutoff
                  AND e.external_reference=@ref
                  AND (SELECT count(*) FROM accounting.journal_lines j
                       WHERE j.journal_entry_id=e.id AND j.organization_id=e.organization_id)=2
                  AND (SELECT SUM(j.debit-j.credit) FROM accounting.journal_lines j
                       WHERE j.journal_entry_id=e.id AND j.organization_id=e.organization_id)=0
                  AND (SELECT count(*) FROM accounting.journal_lines j
                       WHERE j.journal_entry_id=e.id AND j.organization_id=e.organization_id
                         AND j.account_id=@ledger AND j.debit-j.credit=@adjustment)=1
                  AND (SELECT count(*) FROM accounting.journal_lines j
                       WHERE j.journal_entry_id=e.id AND j.organization_id=e.organization_id
                         AND j.account_id=CASE WHEN @adjustment>0
                             THEN @gainAccount ELSE @lossAccount END
                         AND j.debit-j.credit=-@adjustment)=1
                """, ("journal", valuationJournal.Value), ("org", tenant),
                ("cutoff", cutoff),
                ("ref", $"bank-fx-revaluation:v{version}:{account:N}:{cutoff:yyyyMMdd}"),
                ("ledger", bankGl), ("adjustment", adjustment),
                ("gainAccount", gainAccount!.Value),
                ("lossAccount", lossAccount!.Value));
            validValuation = (bool)(await cmd.ExecuteScalarAsync(ct))!;
        }
        else if (validValuation)
            validValuation = adjustment == 0m;
        if (!validValuation) issues.Add("MissingOrIncorrectRevaluationJournal");
        if (actualGl != target || rateId is null)
            issues.Add("ForeignNativeVsPostedBaseGlDifference");

        Guid? maker = null, checker = null;
        string? preparedHash = null, approvedHash = null;
        var reviewSql = amendmentId.HasValue
            ? """
              SELECT p.prepared_by_user_id,p.evidence_sha256,
                     a.approved_by_user_id,a.evidence_sha256
              FROM banking.bank_fx_amendment_preparations p
              LEFT JOIN banking.bank_fx_amendment_approvals a
                ON a.preparation_id=p.id AND a.organization_id=p.organization_id
              WHERE p.organization_id=@org AND p.amendment_id=@amendment
              """
            : """
              SELECT p.prepared_by_user_id,p.evidence_sha256,
                     a.approved_by_user_id,a.evidence_sha256
              FROM banking.bank_fx_close_preparations p
              LEFT JOIN banking.bank_fx_close_approvals a
                ON a.preparation_id=p.id AND a.organization_id=p.organization_id
              WHERE p.organization_id=@org AND p.statement_id=@statement
              """;
        await using (var cmd = Command(cn, tx, reviewSql,
            ("org", tenant), ("statement", statementId),
            ("amendment", (object?)amendmentId ?? DBNull.Value)))
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            if (await reader.ReadAsync(ct))
            {
                maker = reader.GetGuid(0);
                preparedHash = reader.GetString(1);
                if (!reader.IsDBNull(2))
                {
                    checker = reader.GetGuid(2);
                    approvedHash = reader.GetString(3);
                }
            }
        }

        // Fingerprint is over original immutable statement + revaluation IDs
        // and all critical results. A new GL posting changes the fingerprint.
        static string Dec(decimal x) =>
            x.ToString("G29", CultureInfo.InvariantCulture);
        var canonical = string.Join("|",
            tenant.ToString("N"), statementId.ToString("N"),
            account.ToString("N"), cutoff.ToString("yyyyMMdd",
                CultureInfo.InvariantCulture), statementHash,
            rateId?.ToString("N") ?? "", Dec(rate), Dec(native),
            Dec(target), Dec(previousGl), Dec(adjustment),
            valuationJournal?.ToString("N") ?? "", Dec(actualGl),
            invalidLines.ToString(CultureInfo.InvariantCulture),
            unknownJournalLines.ToString(CultureInfo.InvariantCulture),
            chain.Count.ToString(CultureInfo.InvariantCulture),
            string.Join(",", issues.OrderBy(x => x, StringComparer.Ordinal)));
        // Do not change existing v1 approval hashes. Only versioned
        // amendments add their immutable identity to the canonical scope.
        if (amendmentId is Guid currentAmendment)
            canonical += "|amendment:" +
                version.ToString(CultureInfo.InvariantCulture) + ":" +
                currentAmendment.ToString("N");
        var hash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
        var prepared = maker.HasValue && preparedHash == hash;
        var approved = prepared && checker.HasValue &&
            checker.Value != maker!.Value && approvedHash == hash;
        if (maker.HasValue && !prepared) issues.Add("EvidenceChangedAfterPreparation");

        var report = new ForeignBankCloseEvidenceResponse(
            statementId, account, cutoff, currency, native, rate, target,
            actualGl, actualGl - target, chainValid, invalidLines == 0,
            validValuation, unknownJournalLines == 0,
            issues.Count == 0, prepared, approved,
            maker, checker, issues, version);
        return new ForeignCloseSnapshot(report, hash, amendmentId);
    }
}
