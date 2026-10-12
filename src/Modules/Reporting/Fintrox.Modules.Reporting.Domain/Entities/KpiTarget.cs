namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped KpiTarget model. References require application validation.</summary>
public sealed class KpiTarget : OrganizationEntity
{
    public KpiTarget(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid kpiDefinitionId,
        decimal targetValue,
        string measurementUnit,
        DateOnly effectiveFrom)
        : base(id, organizationId, createdAtUtc)
    {
        KpiDefinitionId = DomainRules.Id(kpiDefinitionId, nameof(kpiDefinitionId));
        TargetValue = DomainRules.Nonnegative(targetValue, nameof(targetValue));
        MeasurementUnit = DomainRules.Text(measurementUnit, nameof(measurementUnit));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
    }

    public Guid KpiDefinitionId { get; }
    public decimal TargetValue { get; }
    public string MeasurementUnit { get; }
    public DateOnly EffectiveFrom { get; }
}
