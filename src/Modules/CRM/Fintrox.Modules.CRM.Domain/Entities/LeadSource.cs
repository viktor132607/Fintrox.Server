namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned LeadSource record. Domain invariants enforced at construction.</summary>
public sealed class LeadSource : OrganizationEntity
{
    public LeadSource(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        string channel,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        Channel = DomainRules.Text(channel, nameof(channel));
        IsActive = isActive;
    }

    public string Code { get; }
    public string Name { get; }
    public string Channel { get; }
    public bool IsActive { get; }
}
