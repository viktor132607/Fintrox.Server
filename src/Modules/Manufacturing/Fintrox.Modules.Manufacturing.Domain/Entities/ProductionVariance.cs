namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned ProductionVariance record.</summary>
public sealed class ProductionVariance : OrganizationEntity
{
    public ProductionVariance(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workOrderId,
        string varianceType,
        decimal expectedCost,
        decimal actualCost,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        WorkOrderId = DomainRules.Id(workOrderId, nameof(workOrderId));
        VarianceType = DomainRules.Text(varianceType, nameof(varianceType));
        ExpectedCost = DomainRules.Nonnegative(expectedCost, nameof(expectedCost));
        ActualCost = DomainRules.Nonnegative(actualCost, nameof(actualCost));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid WorkOrderId { get; }
    public string VarianceType { get; }
    public decimal ExpectedCost { get; }
    public decimal ActualCost { get; }
    public string Currency { get; }
}
