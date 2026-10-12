namespace Fintrox.Modules.Consolidation.Domain.Entities;

/// <summary>Tenant-owned ConsolidatedStatementLine; external references validated by application layer.</summary>
public sealed class ConsolidatedStatementLine : OrganizationEntity
{
    public ConsolidatedStatementLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid statementId,
        string accountCode,
        int lineNumber,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        StatementId = DomainRules.Id(statementId, nameof(statementId));
        AccountCode = DomainRules.Text(accountCode, nameof(accountCode));
        LineNumber = DomainRules.PositiveInt(lineNumber, nameof(lineNumber));
        Amount = DomainRules.Nonzero(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid StatementId { get; }
    public string AccountCode { get; }
    public int LineNumber { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
