namespace Fintrox.Modules.Counterparties.Domain.Entities;

/// <summary>Tenant-scoped CounterpartyCreditLimit. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CounterpartyCreditLimit : OrganizationEntity
{
    public CounterpartyCreditLimit(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        decimal creditLimit,
        string currency,
        DateOnly effectiveFrom,
        Guid approvedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        CreditLimit = DomainRules.Nonnegative(creditLimit, nameof(creditLimit));
        Currency = DomainRules.Currency(currency, nameof(currency));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        ApprovedByUserId = DomainRules.Id(approvedByUserId, nameof(approvedByUserId));
    }

    public Guid CounterpartyId { get; }
    public decimal CreditLimit { get; }
    public string Currency { get; }
    public DateOnly EffectiveFrom { get; }
    public Guid ApprovedByUserId { get; }
}
