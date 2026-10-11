namespace Fintrox.Modules.HumanResources.Domain.Entities;

public sealed class PerformanceReview : OrganizationEntity
{
    public PerformanceReview(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        Guid reviewerUserId,
        DateOnly startsOn,
        DateOnly endsOn)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        ReviewerUserId = DomainRules.Id(reviewerUserId, nameof(reviewerUserId));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        if (EndsOn < StartsOn) throw new ArgumentException("The end cannot precede the start.", nameof(endsOn));
    }

    public Guid EmployeeId { get; }
    public Guid ReviewerUserId { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
}
