namespace Fintrox.Modules.Capabilities.Domain.Entities;

/// <summary>Tenant-owned FeatureFlag record. Domain invariants enforced at construction.</summary>
public sealed class FeatureFlag : OrganizationEntity
{
    public FeatureFlag(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string description,
        bool defaultEnabled,
        bool isArchived)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Description = DomainRules.Text(description, nameof(description));
        DefaultEnabled = defaultEnabled;
        IsArchived = isArchived;
    }

    public string Code { get; }
    public string Description { get; }
    public bool DefaultEnabled { get; }
    public bool IsArchived { get; }
}
