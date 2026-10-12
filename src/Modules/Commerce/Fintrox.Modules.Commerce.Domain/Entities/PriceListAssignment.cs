namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned PriceListAssignment; external references validated by application layer.</summary>
public sealed class PriceListAssignment : OrganizationEntity
{
    public PriceListAssignment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid priceListId,
        Guid counterpartyId,
        DateOnly validFrom,
        DateOnly validTo,
        bool isExclusive)
        : base(id, organizationId, createdAtUtc)
    {
        PriceListId = DomainRules.Id(priceListId, nameof(priceListId));
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        ValidFrom = DomainRules.Date(validFrom, nameof(validFrom));
        ValidTo = DomainRules.Date(validTo, nameof(validTo));
        IsExclusive = isExclusive;
        if (ValidTo < ValidFrom) throw new ArgumentException("End must not precede start.", nameof(validTo));
    }

    public Guid PriceListId { get; }
    public Guid CounterpartyId { get; }
    public DateOnly ValidFrom { get; }
    public DateOnly ValidTo { get; }
    public bool IsExclusive { get; }
}
