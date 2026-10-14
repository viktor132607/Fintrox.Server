using System.Data;
using Fintrox.Application.Accounting;
using Fintrox.Contracts.Accounting;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Fintrox.Infrastructure.Accounting;

/// <summary>
/// Strict CSV import and deterministic one-to-one bank/payment candidate
/// automation. Ambiguous candidates are never submitted to the match API.
/// </summary>
public sealed partial class BankReconciliationService
{
    public Task<BankStatementImportResponse?> ImportCsvStatementAsync(
        Guid accountId,
        ImportCsvBankStatementRequest request,
        CancellationToken cancellationToken)
    {
        var parsed = BankStatementCsvParser.Parse(request);
        return ImportStatementAsync(accountId, parsed, cancellationToken);
    }

    public async Task<BankMatchSuggestionsResponse?> GetMatchSuggestionsAsync(
        Guid statementId, CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        if (statementId == Guid.Empty)
            throw new BankReconciliationConflictException(
                "A valid statement ID is required.");

        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(
                IsolationLevel.RepeatableRead, cancellationToken);
            var cn = (NpgsqlConnection)db.Database.GetDbConnection();

            // Read the statement first; no cross-tenant account disclosure.
            await using (var check = Command(cn, tx, """
                SELECT id FROM banking.bank_statements
                WHERE id=@statement AND organization_id=@org
                """, ("statement", statementId), ("org", tenant)))
            {
                if (await check.ExecuteScalarAsync(cancellationToken) is not Guid)
                    return null;
            }

            int unmatched;
            await using (var count = Command(cn, tx, """
                SELECT count(*) FROM banking.bank_statement_lines
                WHERE organization_id=@org AND statement_id=@statement
                  AND matched_payment_id IS NULL
                """, ("org", tenant), ("statement", statementId)))
                unmatched = checked(Convert.ToInt32(
                    await count.ExecuteScalarAsync(cancellationToken)));

            var raw = new List<BankMatchSuggestionResponse>();
            await using (var cmd = Command(cn, tx, """
                SELECT l.id,p.id,l.signed_amount,l.value_date
                FROM banking.bank_statement_lines l
                JOIN banking.bank_statements s
                  ON s.id=l.statement_id AND s.organization_id=l.organization_id
                JOIN banking.bank_accounts a
                  ON a.id=s.bank_account_id AND a.organization_id=s.organization_id
                JOIN payments.payments p
                  ON p.organization_id=l.organization_id
                 AND p.status='Confirmed'
                 AND p.method='BankTransfer'
                 AND p.currency_code=a.currency_code
                 AND p.exchange_rate=1
                 AND p.payment_date=l.value_date
                 AND (CASE WHEN p.direction='Incoming'
                           THEN p.amount ELSE -p.amount END)=l.signed_amount
                WHERE l.organization_id=@org AND l.statement_id=@statement
                  AND l.matched_payment_id IS NULL
                  AND NOT EXISTS (
                      SELECT 1 FROM banking.bank_statement_lines used
                      WHERE used.organization_id=@org
                        AND used.matched_payment_id=p.id
                  )
                  AND 1=(
                      SELECT count(*)
                      FROM accounting.journal_entries e
                      JOIN accounting.journal_lines j
                        ON j.journal_entry_id=e.id AND j.organization_id=e.organization_id
                      WHERE e.organization_id=@org
                        AND e.source='System' AND e.status='Posted'
                        AND e.external_reference='payment:' ||
                            replace(p.id::text,'-','')
                        AND e.posting_date=p.payment_date
                        AND j.account_id=a.gl_account_id
                        AND j.debit-j.credit=l.signed_amount
                  )
                ORDER BY l.id,p.id
                """, ("org", tenant), ("statement", statementId)))
            await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                    raw.Add(new BankMatchSuggestionResponse(
                        reader.GetGuid(0), reader.GetGuid(1),
                        reader.GetDecimal(2), reader.GetFieldValue<DateOnly>(3)));
            }

            // Both ends must be uniquely paired in this entire candidate
            // graph. Two equal payments for a line, or two lines for a payment,
            // remain manual review. Never guess based on database sort order.
            var byLine = raw.GroupBy(x => x.StatementLineId)
                .ToDictionary(x => x.Key, x => x.Count());
            var byPayment = raw.GroupBy(x => x.PaymentId)
                .ToDictionary(x => x.Key, x => x.Count());
            var unique = raw.Where(x =>
                    byLine[x.StatementLineId] == 1 &&
                    byPayment[x.PaymentId] == 1)
                .OrderBy(x => x.ValueDate).ThenBy(x => x.StatementLineId)
                .ToArray();
            await tx.CommitAsync(cancellationToken);
            return new BankMatchSuggestionsResponse(
                statementId, unmatched, unique.Length, unique);
        });
    }

    public async Task<AutoMatchBankStatementResponse?> AutoMatchAsync(
        Guid statementId, CancellationToken cancellationToken)
    {
        var proposals = await GetMatchSuggestionsAsync(
            statementId, cancellationToken);
        if (proposals is null) return null;

        var accepted = 0;
        var already = 0;
        var skipped = new List<Guid>();
        // Each line is committed separately through the same authoritatively
        // validated matching route. A stale/racing proposal cannot bypass
        // MatchLineAsync's locks and unique payment constraint.
        foreach (var proposal in proposals.Suggestions)
        {
            try
            {
                var matched = await MatchLineAsync(
                    statementId, proposal.StatementLineId,
                    proposal.PaymentId, cancellationToken);
                if (matched is null)
                {
                    skipped.Add(proposal.StatementLineId);
                    continue;
                }

                if (matched.AlreadyMatched) already++;
                else accepted++;
            }
            catch (BankReconciliationConflictException)
            {
                skipped.Add(proposal.StatementLineId);
            }
        }

        return new AutoMatchBankStatementResponse(
            statementId, proposals.UnambiguousSuggestions,
            accepted, already, skipped.Count,
            skipped);
    }
}
