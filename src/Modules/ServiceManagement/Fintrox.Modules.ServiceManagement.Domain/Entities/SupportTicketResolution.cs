namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

/// <summary>Tenant-scoped SupportTicketResolution domain record. No cross-module navigation properties.</summary>
public sealed class SupportTicketResolution : OrganizationEntity
{
    public SupportTicketResolution(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supportTicketId,
        string resolutionCode,
        string description,
        DateTimeOffset resolvedAtUtc,
        Guid resolvedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        SupportTicketId = DomainRules.Id(supportTicketId, nameof(supportTicketId));
        ResolutionCode = DomainRules.Text(resolutionCode, nameof(resolutionCode));
        Description = DomainRules.Text(description, nameof(description));
        ResolvedAtUtc = DomainRules.Instant(resolvedAtUtc, nameof(resolvedAtUtc));
        ResolvedByUserId = DomainRules.Id(resolvedByUserId, nameof(resolvedByUserId));
    }

    public Guid SupportTicketId { get; }
    public string ResolutionCode { get; }
    public string Description { get; }
    public DateTimeOffset ResolvedAtUtc { get; }
    public Guid ResolvedByUserId { get; }
}
