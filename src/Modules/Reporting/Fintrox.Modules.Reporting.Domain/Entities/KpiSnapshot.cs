namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped KpiSnapshot model. References require application validation.</summary>
public sealed class KpiSnapshot : OrganizationEntity
{
    public KpiSnapshot(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid kpiDefinitionId,
        DateTimeOffset measuredAtUtc,
        decimal measuredValue,
        decimal targetValue)
        : base(id, organizationId, createdAtUtc)
    {
        KpiDefinitionId = DomainRules.Id(kpiDefinitionId, nameof(kpiDefinitionId));
        MeasuredAtUtc = DomainRules.Instant(measuredAtUtc, nameof(measuredAtUtc));
        MeasuredValue = DomainRules.Nonnegative(measuredValue, nameof(measuredValue));
        TargetValue = DomainRules.Nonnegative(targetValue, nameof(targetValue));
    }

    public Guid KpiDefinitionId { get; }
    public DateTimeOffset MeasuredAtUtc { get; }
    public decimal MeasuredValue { get; }
    public decimal TargetValue { get; }
}
