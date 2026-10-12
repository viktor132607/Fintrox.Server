namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped CashSweepRule record; cross-module references require application authorization.</summary>
public sealed class CashSweepRule : OrganizationEntity
{
    public CashSweepRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid sourceBankAccountId,
        Guid targetBankAccountId,
        decimal thresholdAmount,
        string currency,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        SourceBankAccountId = DomainRules.Id(sourceBankAccountId, nameof(sourceBankAccountId));
        TargetBankAccountId = DomainRules.Id(targetBankAccountId, nameof(targetBankAccountId));
        ThresholdAmount = DomainRules.Nonnegative(thresholdAmount, nameof(thresholdAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        IsActive = isActive;
    }

    public Guid SourceBankAccountId { get; }
    public Guid TargetBankAccountId { get; }
    public decimal ThresholdAmount { get; }
    public string Currency { get; }
    public bool IsActive { get; }
}
