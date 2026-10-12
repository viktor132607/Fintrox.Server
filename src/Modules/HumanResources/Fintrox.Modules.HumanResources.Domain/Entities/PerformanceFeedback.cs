namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned PerformanceFeedback domain model.</summary>
public sealed class PerformanceFeedback : OrganizationEntity
{
    public PerformanceFeedback(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid performanceReviewId,
        Guid reviewerUserId,
        decimal ratingPercent,
        string comment,
        DateTimeOffset submittedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        PerformanceReviewId = DomainRules.Id(performanceReviewId, nameof(performanceReviewId));
        ReviewerUserId = DomainRules.Id(reviewerUserId, nameof(reviewerUserId));
        RatingPercent = DomainRules.Percent(ratingPercent, nameof(ratingPercent));
        Comment = DomainRules.Text(comment, nameof(comment));
        SubmittedAtUtc = DomainRules.Instant(submittedAtUtc, nameof(submittedAtUtc));
    }

    public Guid PerformanceReviewId { get; }
    public Guid ReviewerUserId { get; }
    public decimal RatingPercent { get; }
    public string Comment { get; }
    public DateTimeOffset SubmittedAtUtc { get; }
}
