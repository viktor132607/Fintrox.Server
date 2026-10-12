namespace Fintrox.Modules.Counterparties.Domain.Entities;

/// <summary>Tenant-scoped CounterpartyContactMethod. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CounterpartyContactMethod : OrganizationEntity
{
    public CounterpartyContactMethod(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid contactId,
        string methodType,
        string value,
        bool isPrimary,
        bool isVerified)
        : base(id, organizationId, createdAtUtc)
    {
        ContactId = DomainRules.Id(contactId, nameof(contactId));
        MethodType = DomainRules.Text(methodType, nameof(methodType));
        Value = DomainRules.Text(value, nameof(value));
        IsPrimary = isPrimary;
        IsVerified = isVerified;
    }

    public Guid ContactId { get; }
    public string MethodType { get; }
    public string Value { get; }
    public bool IsPrimary { get; }
    public bool IsVerified { get; }
}
