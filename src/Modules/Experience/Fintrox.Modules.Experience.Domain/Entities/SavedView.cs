namespace Fintrox.Modules.Experience.Domain.Entities;

public sealed class SavedView : OrganizationEntity
{
    public SavedView(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        string resourceType,
        string name,
        string definitionJson)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        ResourceType = DomainRules.Text(resourceType, nameof(resourceType));
        Name = DomainRules.Text(name, nameof(name));
        DefinitionJson = DomainRules.Json(definitionJson, nameof(definitionJson));
    }

    public Guid UserId { get; }
    public string ResourceType { get; }
    public string Name { get; }
    public string DefinitionJson { get; }
}
