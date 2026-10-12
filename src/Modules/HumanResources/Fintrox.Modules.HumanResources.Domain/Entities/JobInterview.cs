namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned JobInterview domain model.</summary>
public sealed class JobInterview : OrganizationEntity
{
    public JobInterview(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid jobApplicationId,
        Guid interviewerUserId,
        DateTimeOffset scheduledAtUtc,
        string interviewType,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        JobApplicationId = DomainRules.Id(jobApplicationId, nameof(jobApplicationId));
        InterviewerUserId = DomainRules.Id(interviewerUserId, nameof(interviewerUserId));
        ScheduledAtUtc = DomainRules.Instant(scheduledAtUtc, nameof(scheduledAtUtc));
        InterviewType = DomainRules.Text(interviewType, nameof(interviewType));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid JobApplicationId { get; }
    public Guid InterviewerUserId { get; }
    public DateTimeOffset ScheduledAtUtc { get; }
    public string InterviewType { get; }
    public string Status { get; }
}
