namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped BankFeeRule record; cross-module references require application authorization.</summary>
public sealed class BankFeeRule : OrganizationEntity
{
    public BankFeeRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid bankAccountId,
        string feeType,
        decimal flatAmount,
        decimal ratePercent,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        BankAccountId = DomainRules.Id(bankAccountId, nameof(bankAccountId));
        FeeType = DomainRules.Text(feeType, nameof(feeType));
        FlatAmount = DomainRules.Nonnegative(flatAmount, nameof(flatAmount));
        RatePercent = DomainRules.Percent(ratePercent, nameof(ratePercent));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid BankAccountId { get; }
    public string FeeType { get; }
    public decimal FlatAmount { get; }
    public decimal RatePercent { get; }
    public string Currency { get; }
}
