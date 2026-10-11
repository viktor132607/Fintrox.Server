namespace Fintrox.Modules.Reporting.Domain.Entities;

public sealed class SavedSearch : OrganizationEntity
{
    public SavedSearch(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid ownerUserId,
        string name,
        string resourceType,
        string filterJson)
        : base(id, organizationId, createdAtUtc)
    {
        OwnerUserId = DomainRules.Id(ownerUserId, nameof(ownerUserId));
        Name = DomainRules.Text(name, nameof(name));
        ResourceType = DomainRules.Text(resourceType, nameof(resourceType));
        FilterJson = DomainRules.Json(filterJson, nameof(filterJson));
    }

    public Guid OwnerUserId { get; }
    public string Name { get; }
    public string ResourceType { get; }
    public string FilterJson { get; }
}
