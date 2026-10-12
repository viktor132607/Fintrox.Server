namespace Fintrox.Modules.Counterparties.Domain.Entities;

/// <summary>Tenant-scoped CounterpartyContactRole. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CounterpartyContactRole : OrganizationEntity
{
    public CounterpartyContactRole(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        Guid contactId,
        string roleCode,
        DateOnly validFrom,
        bool isPrimary)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        ContactId = DomainRules.Id(contactId, nameof(contactId));
        RoleCode = DomainRules.Text(roleCode, nameof(roleCode));
        ValidFrom = DomainRules.Date(validFrom, nameof(validFrom));
        IsPrimary = isPrimary;
    }

    public Guid CounterpartyId { get; }
    public Guid ContactId { get; }
    public string RoleCode { get; }
    public DateOnly ValidFrom { get; }
    public bool IsPrimary { get; }
}
