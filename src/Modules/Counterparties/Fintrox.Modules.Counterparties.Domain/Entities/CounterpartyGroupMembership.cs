namespace Fintrox.Modules.Counterparties.Domain.Entities;

/// <summary>Tenant-scoped CounterpartyGroupMembership. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CounterpartyGroupMembership : OrganizationEntity
{
    public CounterpartyGroupMembership(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        Guid counterpartyGroupId,
        DateOnly joinedOn,
        bool isPrimary)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        CounterpartyGroupId = DomainRules.Id(counterpartyGroupId, nameof(counterpartyGroupId));
        JoinedOn = DomainRules.Date(joinedOn, nameof(joinedOn));
        IsPrimary = isPrimary;
    }

    public Guid CounterpartyId { get; }
    public Guid CounterpartyGroupId { get; }
    public DateOnly JoinedOn { get; }
    public bool IsPrimary { get; }
}
