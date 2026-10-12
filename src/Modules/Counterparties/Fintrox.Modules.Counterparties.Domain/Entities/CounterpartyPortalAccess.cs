namespace Fintrox.Modules.Counterparties.Domain.Entities;

/// <summary>Tenant-scoped CounterpartyPortalAccess. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CounterpartyPortalAccess : OrganizationEntity
{
    public CounterpartyPortalAccess(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        Guid userId,
        string accessRole,
        DateTimeOffset grantedAtUtc,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        UserId = DomainRules.Id(userId, nameof(userId));
        AccessRole = DomainRules.Text(accessRole, nameof(accessRole));
        GrantedAtUtc = DomainRules.Instant(grantedAtUtc, nameof(grantedAtUtc));
        IsActive = isActive;
    }

    public Guid CounterpartyId { get; }
    public Guid UserId { get; }
    public string AccessRole { get; }
    public DateTimeOffset GrantedAtUtc { get; }
    public bool IsActive { get; }
}
