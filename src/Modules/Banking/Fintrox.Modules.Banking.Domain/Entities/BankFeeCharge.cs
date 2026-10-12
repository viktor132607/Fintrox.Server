namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped BankFeeCharge record; cross-module references require application authorization.</summary>
public sealed class BankFeeCharge : OrganizationEntity
{
    public BankFeeCharge(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid bankAccountId,
        Guid statementLineId,
        DateOnly chargedOn,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        BankAccountId = DomainRules.Id(bankAccountId, nameof(bankAccountId));
        StatementLineId = DomainRules.Id(statementLineId, nameof(statementLineId));
        ChargedOn = DomainRules.Date(chargedOn, nameof(chargedOn));
        Amount = DomainRules.Positive(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid BankAccountId { get; }
    public Guid StatementLineId { get; }
    public DateOnly ChargedOn { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
