namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped ReportRun model. References require application validation.</summary>
public sealed class ReportRun : OrganizationEntity
{
    public ReportRun(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid reportDefinitionId,
        Guid requestedByUserId,
        DateTimeOffset requestedAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        ReportDefinitionId = DomainRules.Id(reportDefinitionId, nameof(reportDefinitionId));
        RequestedByUserId = DomainRules.Id(requestedByUserId, nameof(requestedByUserId));
        RequestedAtUtc = DomainRules.Instant(requestedAtUtc, nameof(requestedAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid ReportDefinitionId { get; }
    public Guid RequestedByUserId { get; }
    public DateTimeOffset RequestedAtUtc { get; }
    public string Status { get; }
}
