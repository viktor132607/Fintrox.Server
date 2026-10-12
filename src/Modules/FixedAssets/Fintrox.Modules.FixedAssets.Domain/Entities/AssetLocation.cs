namespace Fintrox.Modules.FixedAssets.Domain.Entities;

/// <summary>Immutable organization-owned AssetLocation domain model.</summary>
public sealed class AssetLocation : OrganizationEntity
{
    public AssetLocation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        Guid responsibleUserId,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        ResponsibleUserId = DomainRules.Id(responsibleUserId, nameof(responsibleUserId));
        IsActive = isActive;
    }

    public string Code { get; }
    public string Name { get; }
    public Guid ResponsibleUserId { get; }
    public bool IsActive { get; }
}
