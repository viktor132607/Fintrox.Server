namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

/// <summary>Organization-scoped immutable ResourceBillingRate model; no cross-module navigations.</summary>
public sealed class ResourceBillingRate : OrganizationEntity
{
    public ResourceBillingRate(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid resourceId,
        Guid customerId,
        decimal hourlyRate,
        string currency,
        DateOnly effectiveFrom)
        : base(id, organizationId, createdAtUtc)
    {
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        CustomerId = DomainRules.Id(customerId, nameof(customerId));
        HourlyRate = DomainRules.Nonnegative(hourlyRate, nameof(hourlyRate));
        Currency = DomainRules.Currency(currency, nameof(currency));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
    }

    public Guid ResourceId { get; }
    public Guid CustomerId { get; }
    public decimal HourlyRate { get; }
    public string Currency { get; }
    public DateOnly EffectiveFrom { get; }
}
