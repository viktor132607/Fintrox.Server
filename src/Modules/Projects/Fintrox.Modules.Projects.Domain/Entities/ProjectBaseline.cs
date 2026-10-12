namespace Fintrox.Modules.Projects.Domain.Entities;

/// <summary>Immutable tenant-scoped ProjectBaseline; cross-module IDs are verified by use cases.</summary>
public sealed class ProjectBaseline : OrganizationEntity
{
    public ProjectBaseline(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid projectId,
        int baselineNumber,
        DateOnly approvedOn,
        decimal approvedBudget,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        ProjectId = DomainRules.Id(projectId, nameof(projectId));
        BaselineNumber = DomainRules.PositiveInt(baselineNumber, nameof(baselineNumber));
        ApprovedOn = DomainRules.Date(approvedOn, nameof(approvedOn));
        ApprovedBudget = DomainRules.Nonnegative(approvedBudget, nameof(approvedBudget));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid ProjectId { get; }
    public int BaselineNumber { get; }
    public DateOnly ApprovedOn { get; }
    public decimal ApprovedBudget { get; }
    public string Currency { get; }
}
