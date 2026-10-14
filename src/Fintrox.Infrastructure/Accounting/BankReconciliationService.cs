using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Fintrox.Application.Accounting;
using Fintrox.Application.Common.Interfaces;
using Fintrox.Contracts.Accounting;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace Fintrox.Infrastructure.Accounting;

/// <summary>
/// Operational PostgreSQL bank evidence import and payment matching.
/// Imported evidence is append-only; only a verified payment match is allowed.
/// Multi-currency statement conversion is deliberately not inferred.
/// </summary>
public sealed class BankReconciliationService(
    FintroxDbContext db,
    ICurrentOrganization organization) : IBankReconciliationService
{
    public async Task<BankAccountResponse> CreateAccountAsync(
        CreateBankAccountRequest request, CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        var iban = new string((request.Iban ?? "").Where(c => !char.IsWhiteSpace(c))
            .ToArray()).ToUpperInvariant();
        var currency = request.CurrencyCode?.Trim().ToUpperInvariant();
        var name = request.Name?.Trim();
        if (iban.Length is < 8 or > 34 ||
            !iban.All(char.IsAsciiLetterOrDigit) ||
            string.IsNullOrWhiteSpace(name) || name.Length > 160 ||
            currency?.Length != 3 || !currency.All(char.IsAsciiLetter) ||
            request.LedgerAccountId == Guid.Empty)
            throw new BankReconciliationConflictException("Invalid bank account fields.");

        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
            var connection = (NpgsqlConnection)db.Database.GetDbConnection();
            var sql = """
                SELECT EXISTS (
                    SELECT 1 FROM core.organizations
                    WHERE id=@org AND is_active=true AND base_currency_code=@currency),
                    EXISTS (
                    SELECT 1 FROM accounting.accounts
                    WHERE id=@account AND organization_id=@org
                      AND type='Asset' AND is_analytical=true AND is_active=true)
                """;
            await using (var check = Command(connection, tx, sql,
                ("org", tenant), ("currency", currency!), ("account", request.LedgerAccountId)))
            await using (var reader = await check.ExecuteReaderAsync(cancellationToken))
            {
                if (!await reader.ReadAsync(cancellationToken) ||
                    !reader.GetBoolean(0) || !reader.GetBoolean(1))
                    throw new BankReconciliationConflictException(
                        "Bank account currency must equal active organization's base currency, and ledger account must be an active analytical asset in that organization.");
            }

            var id = Guid.NewGuid();
            await using (var command = Command(connection, tx, """
                INSERT INTO banking.bank_accounts
                    (id, organization_id, iban, name, currency_code, gl_account_id)
                VALUES (@id, @org, @iban, @name, @currency, @account)
                ON CONFLICT (organization_id, iban) DO NOTHING
                RETURNING id
                """, ("id", id), ("org", tenant), ("iban", iban), ("name", name!),
                ("currency", currency!), ("account", request.LedgerAccountId)))
            {
                if (await command.ExecuteScalarAsync(cancellationToken) is not Guid)
                    throw new BankReconciliationConflictException(
                        "A bank account with this IBAN already exists in the organization.");
            }

            await tx.CommitAsync(cancellationToken);
            return new BankAccountResponse(
                id, iban, name!, currency!, request.LedgerAccountId);
        });
    }

    public async Task<BankStatementImportResponse?> ImportStatementAsync(
        Guid accountId, ImportBankStatementRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        var externalId = request.ExternalId?.Trim();
        if (accountId == Guid.Empty || string.IsNullOrWhiteSpace(externalId) ||
            externalId.Length > 120 || request.FromDate > request.ToDate ||
            request.Lines is null || request.Lines.Count == 0 ||
            request.Lines.Count > 10000)
            throw new BankReconciliationConflictException(
                "Statement reference, ordered dates and 1-10000 lines are required.");

        var lines = request.Lines.Select(line => new BankStatementLineImportRequest(
            (line.ExternalId ?? "").Trim(),
            line.ValueDate, line.SignedAmount, (line.Description ?? "").Trim()))
            .ToArray();
        if (lines.Any(line => line.ExternalId.Length is < 1 or > 120 ||
                line.Description.Length is < 1 or > 500 ||
                line.SignedAmount == 0m ||
                decimal.Round(line.SignedAmount, 4) != line.SignedAmount ||
                line.ValueDate < request.FromDate || line.ValueDate > request.ToDate) ||
            lines.Select(line => line.ExternalId)
                .Distinct(StringComparer.Ordinal).Count() != lines.Length ||
            decimal.Round(request.OpeningBalance, 4) != request.OpeningBalance ||
            decimal.Round(request.ClosingBalance, 4) != request.ClosingBalance ||
            request.OpeningBalance + lines.Sum(line => line.SignedAmount) !=
                request.ClosingBalance)
            throw new BankReconciliationConflictException(
                "Invalid or repeated bank lines, date range, precision or statement opening-to-closing balance.");

        // Canonical exact replay guard, including line ordering and all amounts.
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(new
            {
                ExternalId = externalId, request.FromDate, request.ToDate,
                request.OpeningBalance, request.ClosingBalance, Lines = lines
            })))).ToLowerInvariant();

        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
            var cn = (NpgsqlConnection)db.Database.GetDbConnection();
            await using (var lockAccount = Command(cn, tx, """
                SELECT id FROM banking.bank_accounts
                WHERE id=@account AND organization_id=@org FOR UPDATE
                """, ("org", tenant), ("account", accountId)))
            {
                if (await lockAccount.ExecuteScalarAsync(cancellationToken) is not Guid)
                    return null;
            }

            await using (var existing = Command(cn, tx, """
                SELECT id, payload_sha256,
                    (SELECT count(*) FROM banking.bank_statement_lines
                     WHERE statement_id=s.id AND organization_id=@org)
                FROM banking.bank_statements s
                WHERE s.organization_id=@org AND s.bank_account_id=@account
                  AND s.external_id=@reference
                """, ("org", tenant), ("account", accountId), ("reference", externalId!)))
            await using (var reader = await existing.ExecuteReaderAsync(cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                {
                    var existingId = reader.GetGuid(0);
                    if (reader.GetString(1) != hash)
                        throw new BankReconciliationConflictException(
                            "Statement reference was already imported with different contents.");
                    var oldCount = checked((int)reader.GetInt64(2));
                    await tx.CommitAsync(cancellationToken);
                    return new BankStatementImportResponse(
                        existingId, accountId, externalId!,
                        request.FromDate, request.ToDate, oldCount,
                        request.OpeningBalance, request.ClosingBalance,
                        AlreadyImported: true);
                }
            }

            await using (var overlap = Command(cn, tx, """
                SELECT EXISTS (SELECT 1 FROM banking.bank_statements
                  WHERE organization_id=@org AND bank_account_id=@account
                    AND from_date<=@to AND to_date>=@from)
                """, ("org", tenant), ("account", accountId),
                ("to", request.ToDate), ("from", request.FromDate)))
            {
                if ((bool)(await overlap.ExecuteScalarAsync(cancellationToken))!)
                    throw new BankReconciliationConflictException(
                        "Overlapping statement date ranges for this bank account are not supported.");
            }

            var statementId = Guid.NewGuid();
            await using (var insert = Command(cn, tx, """
                INSERT INTO banking.bank_statements
                    (id,organization_id,bank_account_id,external_id,
                     from_date,to_date,opening_balance,closing_balance,payload_sha256)
                VALUES (@id,@org,@account,@reference,@from,@to,@opening,@closing,@hash)
                """, ("id", statementId), ("org", tenant), ("account", accountId),
                ("reference", externalId!), ("from", request.FromDate),
                ("to", request.ToDate), ("opening", request.OpeningBalance),
                ("closing", request.ClosingBalance), ("hash", hash)))
                await insert.ExecuteNonQueryAsync(cancellationToken);

            foreach (var line in lines)
            {
                await using var insert = Command(cn, tx, """
                    INSERT INTO banking.bank_statement_lines
                        (id,organization_id,statement_id,external_id,
                         value_date,signed_amount,description)
                    VALUES (@id,@org,@statement,@external,@date,@amount,@description)
                    """, ("id", Guid.NewGuid()), ("org", tenant),
                    ("statement", statementId), ("external", line.ExternalId),
                    ("date", line.ValueDate), ("amount", line.SignedAmount),
                    ("description", line.Description));
                await insert.ExecuteNonQueryAsync(cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);
            return new BankStatementImportResponse(
                statementId, accountId, externalId!,
                request.FromDate, request.ToDate, lines.Length,
                request.OpeningBalance, request.ClosingBalance,
                AlreadyImported: false);
        });
    }

    public async Task<BankStatementMatchResponse?> MatchLineAsync(
        Guid statementId, Guid lineId, Guid paymentId,
        CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        if (statementId == Guid.Empty || lineId == Guid.Empty ||
            paymentId == Guid.Empty)
            throw new BankReconciliationConflictException(
                "Statement, line and payment IDs are required.");

        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
            var cn = (NpgsqlConnection)db.Database.GetDbConnection();
            Guid accountId;
            Guid glAccountId;
            string currency;
            decimal amount;
            Guid? matchedId;
            // Account row lock is the global order with statement import.
            await using (var lookup = Command(cn, tx, """
                SELECT a.id,a.gl_account_id,a.currency_code
                FROM banking.bank_statements s
                JOIN banking.bank_accounts a
                  ON a.id=s.bank_account_id AND a.organization_id=s.organization_id
                WHERE s.id=@statement AND s.organization_id=@org
                """, ("statement", statementId), ("org", tenant)))
            await using (var reader = await lookup.ExecuteReaderAsync(cancellationToken))
            {
                if (!await reader.ReadAsync(cancellationToken)) return null;
                accountId = reader.GetGuid(0);
                glAccountId = reader.GetGuid(1);
                currency = reader.GetString(2);
            }
            await using (var bankLock = Command(cn, tx, """
                SELECT id FROM banking.bank_accounts WHERE id=@id
                  AND organization_id=@org FOR UPDATE
                """, ("id", accountId), ("org", tenant)))
                await bankLock.ExecuteScalarAsync(cancellationToken);

            await using (var lineLock = Command(cn, tx, """
                SELECT signed_amount,matched_payment_id
                FROM banking.bank_statement_lines
                WHERE organization_id=@org AND statement_id=@statement
                  AND id=@line FOR UPDATE
                """, ("org", tenant), ("statement", statementId), ("line", lineId)))
            await using (var reader = await lineLock.ExecuteReaderAsync(cancellationToken))
            {
                if (!await reader.ReadAsync(cancellationToken)) return null;
                amount = reader.GetDecimal(0);
                matchedId = reader.IsDBNull(1) ? null : reader.GetGuid(1);
            }
            if (matchedId.HasValue)
            {
                if (matchedId.Value != paymentId)
                    throw new BankReconciliationConflictException(
                        "The bank line is already matched to a different payment.");
                await tx.CommitAsync(cancellationToken);
                return new BankStatementMatchResponse(
                    statementId, lineId, paymentId, AlreadyMatched: true);
            }

            // Require confirmed bank transfer with exact original signed
            // statement amount in the account's base currency.
            await using (var payment = Command(cn, tx, """
                SELECT direction,currency_code,amount,exchange_rate
                FROM payments.payments
                WHERE id=@payment AND organization_id=@org
                  AND status='Confirmed' AND method='BankTransfer'
                FOR SHARE
                """, ("payment", paymentId), ("org", tenant)))
            await using (var reader = await payment.ExecuteReaderAsync(cancellationToken))
            {
                if (!await reader.ReadAsync(cancellationToken))
                    throw new BankReconciliationConflictException(
                        "Payment not found, cancelled, draft or not a bank transfer.");
                var direction = reader.GetString(0);
                var paymentCurrency = reader.GetString(1);
                var originalAmount = reader.GetDecimal(2);
                var rate = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3);
                var signed = direction == "Incoming" ? originalAmount : -originalAmount;
                if (paymentCurrency != currency || signed != amount || rate != 1m)
                    throw new BankReconciliationConflictException(
                        "Statement sign, currency, amount or base exchange rate differs from the confirmed payment.");
            }

            // Payment -> original immutable posted system journal -> exact bank
            // GL account and correct signed account movement, not merely
            // a matching payment amount.
            await using (var evidence = Command(cn, tx, """
                SELECT count(*)
                FROM accounting.journal_entries e
                JOIN accounting.journal_lines l
                    ON l.journal_entry_id=e.id AND l.organization_id=e.organization_id
                WHERE e.organization_id=@org
                  AND e.source='System'
                  AND e.status='Posted'
                  AND e.external_reference=@ref
                  AND l.account_id=@ledger
                  AND (l.debit-l.credit)=@signed
                """, ("org", tenant), ("ref", $"payment:{paymentId:N}"),
                ("ledger", glAccountId), ("signed", amount)))
            {
                if (Convert.ToInt64(
                    await evidence.ExecuteScalarAsync(cancellationToken)) != 1L)
                    throw new BankReconciliationConflictException(
                        "No unique posted payment journal line matches this bank GL account and amount.");
            }

            // Match once across *all* statements for the tenant; the unique
            // index is the final cross-worker concurrency guard.
            await using (var update = Command(cn, tx, """
                UPDATE banking.bank_statement_lines
                SET matched_payment_id=@payment,matched_at_utc=now()
                WHERE id=@line AND statement_id=@statement AND organization_id=@org
                  AND matched_payment_id IS NULL
                """, ("payment", paymentId), ("line", lineId),
                ("statement", statementId), ("org", tenant)))
            {
                try
                {
                    if (await update.ExecuteNonQueryAsync(cancellationToken) != 1)
                        throw new BankReconciliationConflictException(
                            "Bank statement line was matched concurrently.");
                }
                catch (PostgresException exception)
                    when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    throw new BankReconciliationConflictException(
                        "This payment is already matched to another bank statement line.");
                }
            }

            await tx.CommitAsync(cancellationToken);
            return new BankStatementMatchResponse(
                statementId, lineId, paymentId, AlreadyMatched: false);
        });
    }

    public async Task<BankReconciliationResponse?> ReconcileAsync(
        Guid statementId, CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            // Repeatable-read keeps the bank + posted GL evidence in one
            // database snapshot without depending on a particular query order.
            await using var tx = await db.Database.BeginTransactionAsync(
                IsolationLevel.RepeatableRead, cancellationToken);
            var cn = (NpgsqlConnection)db.Database.GetDbConnection();
            Guid accountId;
            Guid glAccountId;
            string currency;
            DateOnly endDate;
            decimal opening;
            decimal closing;
            await using (var cmd = Command(cn, tx, """
                SELECT a.id,a.gl_account_id,a.currency_code,
                    s.to_date,s.opening_balance,s.closing_balance
                FROM banking.bank_statements s
                JOIN banking.bank_accounts a
                  ON a.id=s.bank_account_id AND a.organization_id=s.organization_id
                WHERE s.id=@id AND s.organization_id=@org
                """, ("id", statementId), ("org", tenant)))
            await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                if (!await reader.ReadAsync(cancellationToken)) return null;
                accountId = reader.GetGuid(0);
                glAccountId = reader.GetGuid(1);
                currency = reader.GetString(2);
                endDate = reader.GetFieldValue<DateOnly>(3);
                opening = reader.GetDecimal(4);
                closing = reader.GetDecimal(5);
            }

            var unmatched = new List<Guid>();
            int matched = 0;
            decimal movement = 0;
            await using (var cmd = Command(cn, tx, """
                SELECT id,signed_amount,matched_payment_id
                FROM banking.bank_statement_lines
                WHERE statement_id=@id AND organization_id=@org
                ORDER BY value_date,id
                """, ("id", statementId), ("org", tenant)))
            await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    var lineId = reader.GetGuid(0);
                    movement += reader.GetDecimal(1);
                    var valid = false;
                    if (!reader.IsDBNull(2))
                    {
                        // Check current state in a separate query below; don't
                        // count invalid/cancelled matches as reconciled.
                        matched++;
                    }
                    else unmatched.Add(lineId);
                }
            }

            // A matched payment that was later cancelled, reversed, or whose
            // GL control line changed is no longer valid banking evidence.
            await using (var cmd = Command(cn, tx, """
                SELECT l.id
                FROM banking.bank_statement_lines l
                JOIN payments.payments p
                    ON p.id=l.matched_payment_id AND p.organization_id=l.organization_id
                WHERE l.organization_id=@org AND l.statement_id=@id
                  AND (p.status <> 'Confirmed' OR p.method <> 'BankTransfer'
                       OR p.currency_code <> @currency
                       OR (CASE WHEN p.direction='Incoming' THEN p.amount
                                ELSE -p.amount END) <> l.signed_amount
                       OR p.exchange_rate <> 1
                       OR NOT EXISTS (
                           SELECT 1 FROM accounting.journal_entries e
                           JOIN accounting.journal_lines j
                             ON j.journal_entry_id=e.id AND j.organization_id=e.organization_id
                           WHERE e.organization_id=@org AND e.source='System'
                             AND e.status='Posted'
                             AND e.external_reference='payment:' ||
                                replace(p.id::text, '-', '')
                             AND j.account_id=@ledger
                             AND (j.debit-j.credit)=l.signed_amount
                       ))
                """, ("org", tenant), ("id", statementId),
                ("currency", currency), ("ledger", glAccountId)))
            await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    matched--;
                    unmatched.Add(reader.GetGuid(0));
                }
            }

            decimal glBalance;
            await using (var cmd = Command(cn, tx, """
                SELECT COALESCE(sum(j.debit-j.credit),0)
                FROM accounting.journal_lines j
                JOIN accounting.journal_entries e
                  ON e.id=j.journal_entry_id AND e.organization_id=j.organization_id
                WHERE j.organization_id=@org AND j.account_id=@ledger
                  AND e.posting_date<=@end
                  AND e.status IN ('Posted','Reversed')
                """, ("org", tenant), ("ledger", glAccountId), ("end", endDate)))
                glBalance = (decimal)(await cmd.ExecuteScalarAsync(cancellationToken))!;

            var bookUncleared = new List<Guid>();
            await using (var cmd = Command(cn, tx, """
                SELECT p.id
                FROM payments.payments p
                WHERE p.organization_id=@org AND p.status='Confirmed'
                  AND p.method='BankTransfer'
                  AND p.currency_code=@currency
                  AND p.payment_date<=@end
                  AND p.exchange_rate=1
                  AND EXISTS (
                     SELECT 1 FROM accounting.journal_entries e
                     JOIN accounting.journal_lines j
                       ON j.journal_entry_id=e.id AND j.organization_id=e.organization_id
                     WHERE e.organization_id=@org AND e.status='Posted'
                       AND e.source='System'
                       AND e.external_reference='payment:' ||
                           replace(p.id::text, '-', '')
                       AND j.account_id=@ledger
                  )
                  AND NOT EXISTS (
                     SELECT 1 FROM banking.bank_statement_lines l
                     JOIN banking.bank_statements s
                       ON s.id=l.statement_id AND s.organization_id=l.organization_id
                     WHERE l.organization_id=@org
                       AND s.bank_account_id=@account
                       AND s.to_date<=@end AND l.matched_payment_id=p.id
                  )
                ORDER BY p.payment_date,p.id
                """, ("org", tenant), ("currency", currency),
                ("end", endDate), ("ledger", glAccountId),
                ("account", accountId)))
            await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                    bookUncleared.Add(reader.GetGuid(0));
            }
            var expectedClosing = opening + movement;
            var statementBalanced = expectedClosing == closing;
            var difference = closing - glBalance;
            var matchedFully = unmatched.Count == 0;
            await tx.CommitAsync(cancellationToken);
            return new BankReconciliationResponse(
                statementId, accountId, currency, endDate, opening, closing,
                expectedClosing, glBalance, difference,
                matched + unmatched.Count, matched, unmatched.Count,
                bookUncleared.Count,
                statementBalanced, matchedFully,
                difference == 0m,
                statementBalanced && matchedFully && difference == 0m &&
                    bookUncleared.Count == 0,
                unmatched, bookUncleared);
        });
    }

    private static NpgsqlCommand Command(
        NpgsqlConnection cn, IDbContextTransaction transaction, string sql,
        params (string Name, object Value)[] parameters)
    {
        var command = new NpgsqlCommand(
            sql, cn, (NpgsqlTransaction)transaction.GetDbTransaction());
        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value);
        return command;
    }
}
