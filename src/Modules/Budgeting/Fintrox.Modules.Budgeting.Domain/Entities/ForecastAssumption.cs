namespace Fintrox.Modules.Budgeting.Domain.Entities;

/// <summary>Tenant-owned ForecastAssumption record. Domain invariants enforced at construction.</summary>
public sealed class ForecastAssumption : OrganizationEntity
{
    public ForecastAssumption(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid forecastVersionId,
        string code,
        string description,
        decimal assumedValue,
        string unit)
        : base(id, organizationId, createdAtUtc)
    {
        ForecastVersionId = DomainRules.Id(forecastVersionId, nameof(forecastVersionId));
        Code = DomainRules.Text(code, nameof(code));
        Description = DomainRules.Text(description, nameof(description));
        AssumedValue = DomainRules.Nonnegative(assumedValue, nameof(assumedValue));
        Unit = DomainRules.Text(unit, nameof(unit));
    }

    public Guid ForecastVersionId { get; }
    public string Code { get; }
    public string Description { get; }
    public decimal AssumedValue { get; }
    public string Unit { get; }
}
