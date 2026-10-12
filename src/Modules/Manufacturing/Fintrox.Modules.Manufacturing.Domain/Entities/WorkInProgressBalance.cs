namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned WorkInProgressBalance record.</summary>
public sealed class WorkInProgressBalance : OrganizationEntity
{
    public WorkInProgressBalance(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workOrderId,
        DateOnly asOfDate,
        decimal materialCost,
        decimal laborCost,
        decimal overheadCost,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        WorkOrderId = DomainRules.Id(workOrderId, nameof(workOrderId));
        AsOfDate = DomainRules.Date(asOfDate, nameof(asOfDate));
        MaterialCost = DomainRules.Nonnegative(materialCost, nameof(materialCost));
        LaborCost = DomainRules.Nonnegative(laborCost, nameof(laborCost));
        OverheadCost = DomainRules.Nonnegative(overheadCost, nameof(overheadCost));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid WorkOrderId { get; }
    public DateOnly AsOfDate { get; }
    public decimal MaterialCost { get; }
    public decimal LaborCost { get; }
    public decimal OverheadCost { get; }
    public string Currency { get; }
}
