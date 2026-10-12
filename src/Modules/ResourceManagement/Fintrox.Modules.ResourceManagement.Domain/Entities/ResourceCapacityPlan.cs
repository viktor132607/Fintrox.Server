namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

/// <summary>Organization-scoped immutable ResourceCapacityPlan model; no cross-module navigations.</summary>
public sealed class ResourceCapacityPlan : OrganizationEntity
{
    public ResourceCapacityPlan(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid resourceId,
        DateOnly periodStartsOn,
        DateOnly periodEndsOn,
        decimal capacityHours)
        : base(id, organizationId, createdAtUtc)
    {
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        PeriodStartsOn = DomainRules.Date(periodStartsOn, nameof(periodStartsOn));
        PeriodEndsOn = DomainRules.Date(periodEndsOn, nameof(periodEndsOn));
        CapacityHours = DomainRules.Nonnegative(capacityHours, nameof(capacityHours));
        if (PeriodEndsOn < PeriodStartsOn) throw new ArgumentException("End must not precede start.", nameof(periodEndsOn));
    }

    public Guid ResourceId { get; }
    public DateOnly PeriodStartsOn { get; }
    public DateOnly PeriodEndsOn { get; }
    public decimal CapacityHours { get; }
}
