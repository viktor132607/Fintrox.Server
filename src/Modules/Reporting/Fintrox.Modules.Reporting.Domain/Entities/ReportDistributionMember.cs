namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped ReportDistributionMember model. References require application validation.</summary>
public sealed class ReportDistributionMember : OrganizationEntity
{
    public ReportDistributionMember(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid distributionListId,
        Guid recipientUserId,
        DateOnly joinedOn)
        : base(id, organizationId, createdAtUtc)
    {
        DistributionListId = DomainRules.Id(distributionListId, nameof(distributionListId));
        RecipientUserId = DomainRules.Id(recipientUserId, nameof(recipientUserId));
        JoinedOn = DomainRules.Date(joinedOn, nameof(joinedOn));
    }

    public Guid DistributionListId { get; }
    public Guid RecipientUserId { get; }
    public DateOnly JoinedOn { get; }
}
