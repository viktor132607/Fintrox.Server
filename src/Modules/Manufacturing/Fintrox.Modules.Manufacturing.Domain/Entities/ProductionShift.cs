namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned ProductionShift record.</summary>
public sealed class ProductionShift : OrganizationEntity
{
    public ProductionShift(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workCenterId,
        string shiftCode,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        decimal capacityHours)
        : base(id, organizationId, createdAtUtc)
    {
        WorkCenterId = DomainRules.Id(workCenterId, nameof(workCenterId));
        ShiftCode = DomainRules.Text(shiftCode, nameof(shiftCode));
        StartsAtUtc = DomainRules.Instant(startsAtUtc, nameof(startsAtUtc));
        EndsAtUtc = DomainRules.Instant(endsAtUtc, nameof(endsAtUtc));
        CapacityHours = DomainRules.Positive(capacityHours, nameof(capacityHours));
    }

    public Guid WorkCenterId { get; }
    public string ShiftCode { get; }
    public DateTimeOffset StartsAtUtc { get; }
    public DateTimeOffset EndsAtUtc { get; }
    public decimal CapacityHours { get; }
}
