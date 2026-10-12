namespace Fintrox.Modules.Experience.Domain.Entities;

/// <summary>Immutable organization-owned UserLayout domain model.</summary>
public sealed class UserLayout : OrganizationEntity
{
    public UserLayout(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        Guid layoutTemplateId,
        string name,
        bool isDefault)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        LayoutTemplateId = DomainRules.Id(layoutTemplateId, nameof(layoutTemplateId));
        Name = DomainRules.Text(name, nameof(name));
        IsDefault = isDefault;
    }

    public Guid UserId { get; }
    public Guid LayoutTemplateId { get; }
    public string Name { get; }
    public bool IsDefault { get; }
}
