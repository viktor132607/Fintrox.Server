namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

/// <summary>Organization-scoped immutable ResourceCapacityAdjustment model; no cross-module navigations.</summary>
public sealed class ResourceCapacityAdjustment : OrganizationEntity
{
    public ResourceCapacityAdjustment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid resourceId,
        DateOnly adjustmentOn,
        decimal hoursChange,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        AdjustmentOn = DomainRules.Date(adjustmentOn, nameof(adjustmentOn));
        HoursChange = DomainRules.Nonzero(hoursChange, nameof(hoursChange));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid ResourceId { get; }
    public DateOnly AdjustmentOn { get; }
    public decimal HoursChange { get; }
    public string Reason { get; }
}
