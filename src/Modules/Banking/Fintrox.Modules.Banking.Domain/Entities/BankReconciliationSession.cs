namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped BankReconciliationSession record; cross-module references require application authorization.</summary>
public sealed class BankReconciliationSession : OrganizationEntity
{
    public BankReconciliationSession(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid bankAccountId,
        DateOnly fromDate,
        DateOnly toDate,
        decimal statementEndingBalance,
        string currency,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        BankAccountId = DomainRules.Id(bankAccountId, nameof(bankAccountId));
        FromDate = DomainRules.Date(fromDate, nameof(fromDate));
        ToDate = DomainRules.Date(toDate, nameof(toDate));
        StatementEndingBalance = DomainRules.Nonnegative(statementEndingBalance, nameof(statementEndingBalance));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Status = DomainRules.Text(status, nameof(status));
        if (ToDate < FromDate) throw new ArgumentException("End must not precede start.", nameof(toDate));
    }

    public Guid BankAccountId { get; }
    public DateOnly FromDate { get; }
    public DateOnly ToDate { get; }
    public decimal StatementEndingBalance { get; }
    public string Currency { get; }
    public string Status { get; }
}
