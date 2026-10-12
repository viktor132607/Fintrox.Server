namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

/// <summary>Organization-scoped immutable ResourceForecast model; no cross-module navigations.</summary>
public sealed class ResourceForecast : OrganizationEntity
{
    public ResourceForecast(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid resourcePoolId,
        DateOnly forecastOn,
        decimal requiredHours,
        decimal availableHours)
        : base(id, organizationId, createdAtUtc)
    {
        ResourcePoolId = DomainRules.Id(resourcePoolId, nameof(resourcePoolId));
        ForecastOn = DomainRules.Date(forecastOn, nameof(forecastOn));
        RequiredHours = DomainRules.Nonnegative(requiredHours, nameof(requiredHours));
        AvailableHours = DomainRules.Nonnegative(availableHours, nameof(availableHours));
    }

    public Guid ResourcePoolId { get; }
    public DateOnly ForecastOn { get; }
    public decimal RequiredHours { get; }
    public decimal AvailableHours { get; }
}
