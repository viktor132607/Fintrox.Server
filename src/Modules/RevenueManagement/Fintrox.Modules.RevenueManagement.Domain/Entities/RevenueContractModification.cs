namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

/// <summary>Organization-scoped immutable RevenueContractModification model; no cross-module navigations.</summary>
public sealed class RevenueContractModification : OrganizationEntity
{
    public RevenueContractModification(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid revenueContractId,
        DateOnly modificationDate,
        string reason,
        decimal changeAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        RevenueContractId = DomainRules.Id(revenueContractId, nameof(revenueContractId));
        ModificationDate = DomainRules.Date(modificationDate, nameof(modificationDate));
        Reason = DomainRules.Text(reason, nameof(reason));
        ChangeAmount = DomainRules.Nonzero(changeAmount, nameof(changeAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid RevenueContractId { get; }
    public DateOnly ModificationDate { get; }
    public string Reason { get; }
    public decimal ChangeAmount { get; }
    public string Currency { get; }
}
