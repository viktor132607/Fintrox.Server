namespace Fintrox.Modules.Projects.Domain.Entities;

/// <summary>Immutable tenant-scoped ProjectForecast; cross-module IDs are verified by use cases.</summary>
public sealed class ProjectForecast : OrganizationEntity
{
    public ProjectForecast(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid projectId,
        DateOnly forecastOn,
        DateOnly estimatedCompletionOn,
        decimal forecastCost,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        ProjectId = DomainRules.Id(projectId, nameof(projectId));
        ForecastOn = DomainRules.Date(forecastOn, nameof(forecastOn));
        EstimatedCompletionOn = DomainRules.Date(estimatedCompletionOn, nameof(estimatedCompletionOn));
        ForecastCost = DomainRules.Nonnegative(forecastCost, nameof(forecastCost));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid ProjectId { get; }
    public DateOnly ForecastOn { get; }
    public DateOnly EstimatedCompletionOn { get; }
    public decimal ForecastCost { get; }
    public string Currency { get; }
}
