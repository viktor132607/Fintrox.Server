namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned OpportunityForecast record. Domain invariants enforced at construction.</summary>
public sealed class OpportunityForecast : OrganizationEntity
{
    public OpportunityForecast(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid opportunityId,
        DateOnly forecastOn,
        decimal amount,
        decimal probabilityPercent,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        OpportunityId = DomainRules.Id(opportunityId, nameof(opportunityId));
        ForecastOn = DomainRules.Date(forecastOn, nameof(forecastOn));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        ProbabilityPercent = DomainRules.Percent(probabilityPercent, nameof(probabilityPercent));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid OpportunityId { get; }
    public DateOnly ForecastOn { get; }
    public decimal Amount { get; }
    public decimal ProbabilityPercent { get; }
    public string Currency { get; }
}
