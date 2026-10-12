namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

/// <summary>Tenant-scoped SupportTicketCategory domain record. No cross-module navigation properties.</summary>
public sealed class SupportTicketCategory : OrganizationEntity
{
    public SupportTicketCategory(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        string defaultPriority,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        DefaultPriority = DomainRules.Text(defaultPriority, nameof(defaultPriority));
        IsActive = isActive;
    }

    public string Code { get; }
    public string Name { get; }
    public string DefaultPriority { get; }
    public bool IsActive { get; }
}
