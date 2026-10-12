namespace Fintrox.Modules.Budgeting.Domain.Entities;

/// <summary>Tenant-owned RollingForecastWindow record. Domain invariants enforced at construction.</summary>
public sealed class RollingForecastWindow : OrganizationEntity
{
    public RollingForecastWindow(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid forecastId,
        DateOnly startsOn,
        DateOnly endsOn,
        int intervalMonths)
        : base(id, organizationId, createdAtUtc)
    {
        ForecastId = DomainRules.Id(forecastId, nameof(forecastId));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        IntervalMonths = DomainRules.PositiveInt(intervalMonths, nameof(intervalMonths));
        if (EndsOn < StartsOn) throw new ArgumentException("End must not precede start.", nameof(endsOn));
    }

    public Guid ForecastId { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public int IntervalMonths { get; }
}
