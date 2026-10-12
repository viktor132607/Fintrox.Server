namespace Fintrox.Modules.QualityManagement.Domain.Entities;

/// <summary>Immutable, organization-scoped QualityRelease model. References require application validation.</summary>
public sealed class QualityRelease : OrganizationEntity
{
    public QualityRelease(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inspectionBatchId,
        Guid approvedByUserId,
        DateOnly releasedOn,
        string releaseStatus)
        : base(id, organizationId, createdAtUtc)
    {
        InspectionBatchId = DomainRules.Id(inspectionBatchId, nameof(inspectionBatchId));
        ApprovedByUserId = DomainRules.Id(approvedByUserId, nameof(approvedByUserId));
        ReleasedOn = DomainRules.Date(releasedOn, nameof(releasedOn));
        ReleaseStatus = DomainRules.Text(releaseStatus, nameof(releaseStatus));
    }

    public Guid InspectionBatchId { get; }
    public Guid ApprovedByUserId { get; }
    public DateOnly ReleasedOn { get; }
    public string ReleaseStatus { get; }
}
