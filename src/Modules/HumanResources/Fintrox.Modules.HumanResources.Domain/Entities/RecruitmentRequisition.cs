namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned RecruitmentRequisition domain model.</summary>
public sealed class RecruitmentRequisition : OrganizationEntity
{
    public RecruitmentRequisition(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid positionId,
        Guid requestedByUserId,
        DateOnly requestedOn,
        int openings,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        PositionId = DomainRules.Id(positionId, nameof(positionId));
        RequestedByUserId = DomainRules.Id(requestedByUserId, nameof(requestedByUserId));
        RequestedOn = DomainRules.Date(requestedOn, nameof(requestedOn));
        Openings = DomainRules.PositiveInt(openings, nameof(openings));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid PositionId { get; }
    public Guid RequestedByUserId { get; }
    public DateOnly RequestedOn { get; }
    public int Openings { get; }
    public string Status { get; }
}
