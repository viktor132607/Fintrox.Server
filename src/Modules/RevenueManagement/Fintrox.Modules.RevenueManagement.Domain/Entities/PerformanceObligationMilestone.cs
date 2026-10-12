namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

/// <summary>Organization-scoped immutable PerformanceObligationMilestone model; no cross-module navigations.</summary>
public sealed class PerformanceObligationMilestone : OrganizationEntity
{
    public PerformanceObligationMilestone(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid performanceObligationId,
        string milestoneCode,
        DateOnly dueOn,
        decimal completionPercent)
        : base(id, organizationId, createdAtUtc)
    {
        PerformanceObligationId = DomainRules.Id(performanceObligationId, nameof(performanceObligationId));
        MilestoneCode = DomainRules.Text(milestoneCode, nameof(milestoneCode));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
        CompletionPercent = DomainRules.Percent(completionPercent, nameof(completionPercent));
    }

    public Guid PerformanceObligationId { get; }
    public string MilestoneCode { get; }
    public DateOnly DueOn { get; }
    public decimal CompletionPercent { get; }
}
