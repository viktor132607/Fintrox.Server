namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned CrmSegment record. Domain invariants enforced at construction.</summary>
public sealed class CrmSegment : OrganizationEntity
{
    public CrmSegment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string name,
        string filterJson,
        Guid createdByUserId,
        bool isDynamic)
        : base(id, organizationId, createdAtUtc)
    {
        Name = DomainRules.Text(name, nameof(name));
        FilterJson = DomainRules.Json(filterJson, nameof(filterJson));
        CreatedByUserId = DomainRules.Id(createdByUserId, nameof(createdByUserId));
        IsDynamic = isDynamic;
    }

    public string Name { get; }
    public string FilterJson { get; }
    public Guid CreatedByUserId { get; }
    public bool IsDynamic { get; }
}
