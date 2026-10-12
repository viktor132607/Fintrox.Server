namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned CrmTerritoryAssignment record. Domain invariants enforced at construction.</summary>
public sealed class CrmTerritoryAssignment : OrganizationEntity
{
    public CrmTerritoryAssignment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid territoryId,
        Guid userId,
        DateOnly effectiveFrom,
        bool isPrimary)
        : base(id, organizationId, createdAtUtc)
    {
        TerritoryId = DomainRules.Id(territoryId, nameof(territoryId));
        UserId = DomainRules.Id(userId, nameof(userId));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        IsPrimary = isPrimary;
    }

    public Guid TerritoryId { get; }
    public Guid UserId { get; }
    public DateOnly EffectiveFrom { get; }
    public bool IsPrimary { get; }
}
