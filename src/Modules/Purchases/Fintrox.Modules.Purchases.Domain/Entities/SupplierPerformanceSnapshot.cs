namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped SupplierPerformanceSnapshot model. References require application validation.</summary>
public sealed class SupplierPerformanceSnapshot : OrganizationEntity
{
    public SupplierPerformanceSnapshot(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supplierId,
        DateOnly periodStartsOn,
        DateOnly periodEndsOn,
        decimal onTimeRatePercent,
        decimal defectRatePercent)
        : base(id, organizationId, createdAtUtc)
    {
        SupplierId = DomainRules.Id(supplierId, nameof(supplierId));
        PeriodStartsOn = DomainRules.Date(periodStartsOn, nameof(periodStartsOn));
        PeriodEndsOn = DomainRules.Date(periodEndsOn, nameof(periodEndsOn));
        OnTimeRatePercent = DomainRules.Percent(onTimeRatePercent, nameof(onTimeRatePercent));
        DefectRatePercent = DomainRules.Percent(defectRatePercent, nameof(defectRatePercent));
        if (PeriodEndsOn < PeriodStartsOn) throw new ArgumentException("End must not precede start.", nameof(periodEndsOn));
    }

    public Guid SupplierId { get; }
    public DateOnly PeriodStartsOn { get; }
    public DateOnly PeriodEndsOn { get; }
    public decimal OnTimeRatePercent { get; }
    public decimal DefectRatePercent { get; }
}
