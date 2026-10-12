namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

/// <summary>Organization-scoped immutable ResourceUtilizationSnapshot model; no cross-module navigations.</summary>
public sealed class ResourceUtilizationSnapshot : OrganizationEntity
{
    public ResourceUtilizationSnapshot(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid resourceId,
        DateOnly periodStartsOn,
        DateOnly periodEndsOn,
        decimal billableHours,
        decimal totalHours)
        : base(id, organizationId, createdAtUtc)
    {
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        PeriodStartsOn = DomainRules.Date(periodStartsOn, nameof(periodStartsOn));
        PeriodEndsOn = DomainRules.Date(periodEndsOn, nameof(periodEndsOn));
        BillableHours = DomainRules.Nonnegative(billableHours, nameof(billableHours));
        TotalHours = DomainRules.Nonnegative(totalHours, nameof(totalHours));
        if (PeriodEndsOn < PeriodStartsOn) throw new ArgumentException("End must not precede start.", nameof(periodEndsOn));
    }

    public Guid ResourceId { get; }
    public DateOnly PeriodStartsOn { get; }
    public DateOnly PeriodEndsOn { get; }
    public decimal BillableHours { get; }
    public decimal TotalHours { get; }
}
