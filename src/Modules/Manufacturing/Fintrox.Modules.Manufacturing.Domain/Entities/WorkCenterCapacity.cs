namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned WorkCenterCapacity record.</summary>
public sealed class WorkCenterCapacity : OrganizationEntity
{
    public WorkCenterCapacity(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workCenterId,
        DateOnly capacityDate,
        decimal availableHours,
        decimal reservedHours)
        : base(id, organizationId, createdAtUtc)
    {
        WorkCenterId = DomainRules.Id(workCenterId, nameof(workCenterId));
        CapacityDate = DomainRules.Date(capacityDate, nameof(capacityDate));
        AvailableHours = DomainRules.Nonnegative(availableHours, nameof(availableHours));
        ReservedHours = DomainRules.Nonnegative(reservedHours, nameof(reservedHours));
    }

    public Guid WorkCenterId { get; }
    public DateOnly CapacityDate { get; }
    public decimal AvailableHours { get; }
    public decimal ReservedHours { get; }
}
