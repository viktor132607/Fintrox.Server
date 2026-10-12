namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped BankStatementBalance record; cross-module references require application authorization.</summary>
public sealed class BankStatementBalance : OrganizationEntity
{
    public BankStatementBalance(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid statementId,
        DateOnly balanceDate,
        decimal availableAmount,
        decimal bookedAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        StatementId = DomainRules.Id(statementId, nameof(statementId));
        BalanceDate = DomainRules.Date(balanceDate, nameof(balanceDate));
        AvailableAmount = DomainRules.Nonnegative(availableAmount, nameof(availableAmount));
        BookedAmount = DomainRules.Nonnegative(bookedAmount, nameof(bookedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid StatementId { get; }
    public DateOnly BalanceDate { get; }
    public decimal AvailableAmount { get; }
    public decimal BookedAmount { get; }
    public string Currency { get; }
}
