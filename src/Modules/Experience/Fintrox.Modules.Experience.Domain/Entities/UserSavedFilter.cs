namespace Fintrox.Modules.Experience.Domain.Entities;

/// <summary>Immutable organization-owned UserSavedFilter domain model.</summary>
public sealed class UserSavedFilter : OrganizationEntity
{
    public UserSavedFilter(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        string moduleCode,
        string name,
        string filterJson,
        bool isDefault)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        ModuleCode = DomainRules.Text(moduleCode, nameof(moduleCode));
        Name = DomainRules.Text(name, nameof(name));
        FilterJson = DomainRules.Json(filterJson, nameof(filterJson));
        IsDefault = isDefault;
    }

    public Guid UserId { get; }
    public string ModuleCode { get; }
    public string Name { get; }
    public string FilterJson { get; }
    public bool IsDefault { get; }
}
