namespace Fintrox.Modules.Budgeting.Domain.Entities;

/// <summary>Tenant-owned ForecastLine record. Domain invariants enforced at construction.</summary>
public sealed class ForecastLine : OrganizationEntity
{
    public ForecastLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid forecastVersionId,
        Guid accountId,
        DateOnly periodStartsOn,
        DateOnly periodEndsOn,
        decimal forecastAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        ForecastVersionId = DomainRules.Id(forecastVersionId, nameof(forecastVersionId));
        AccountId = DomainRules.Id(accountId, nameof(accountId));
        PeriodStartsOn = DomainRules.Date(periodStartsOn, nameof(periodStartsOn));
        PeriodEndsOn = DomainRules.Date(periodEndsOn, nameof(periodEndsOn));
        ForecastAmount = DomainRules.Nonnegative(forecastAmount, nameof(forecastAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        if (PeriodEndsOn < PeriodStartsOn) throw new ArgumentException("End must not precede start.", nameof(periodEndsOn));
    }

    public Guid ForecastVersionId { get; }
    public Guid AccountId { get; }
    public DateOnly PeriodStartsOn { get; }
    public DateOnly PeriodEndsOn { get; }
    public decimal ForecastAmount { get; }
    public string Currency { get; }
}
