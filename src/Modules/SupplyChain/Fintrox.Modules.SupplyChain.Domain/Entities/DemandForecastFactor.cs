namespace Fintrox.Modules.SupplyChain.Domain.Entities;

/// <summary>Tenant-scoped DemandForecastFactor domain record. No cross-module navigation properties.</summary>
public sealed class DemandForecastFactor : OrganizationEntity
{
    public DemandForecastFactor(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid demandForecastId,
        string factorCode,
        decimal weightPercent,
        decimal value)
        : base(id, organizationId, createdAtUtc)
    {
        DemandForecastId = DomainRules.Id(demandForecastId, nameof(demandForecastId));
        FactorCode = DomainRules.Text(factorCode, nameof(factorCode));
        WeightPercent = DomainRules.Percent(weightPercent, nameof(weightPercent));
        Value = DomainRules.Nonnegative(value, nameof(value));
    }

    public Guid DemandForecastId { get; }
    public string FactorCode { get; }
    public decimal WeightPercent { get; }
    public decimal Value { get; }
}
