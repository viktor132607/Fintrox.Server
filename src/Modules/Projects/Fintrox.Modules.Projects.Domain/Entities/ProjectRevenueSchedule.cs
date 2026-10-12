namespace Fintrox.Modules.Projects.Domain.Entities;

/// <summary>Immutable tenant-scoped ProjectRevenueSchedule; cross-module IDs are verified by use cases.</summary>
public sealed class ProjectRevenueSchedule : OrganizationEntity
{
    public ProjectRevenueSchedule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid projectId,
        Guid milestoneId,
        decimal expectedRevenue,
        string currency,
        DateOnly expectedOn)
        : base(id, organizationId, createdAtUtc)
    {
        ProjectId = DomainRules.Id(projectId, nameof(projectId));
        MilestoneId = DomainRules.Id(milestoneId, nameof(milestoneId));
        ExpectedRevenue = DomainRules.Nonnegative(expectedRevenue, nameof(expectedRevenue));
        Currency = DomainRules.Currency(currency, nameof(currency));
        ExpectedOn = DomainRules.Date(expectedOn, nameof(expectedOn));
    }

    public Guid ProjectId { get; }
    public Guid MilestoneId { get; }
    public decimal ExpectedRevenue { get; }
    public string Currency { get; }
    public DateOnly ExpectedOn { get; }
}
