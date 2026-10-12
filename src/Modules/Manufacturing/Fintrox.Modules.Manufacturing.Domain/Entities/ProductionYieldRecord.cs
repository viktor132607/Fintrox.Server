namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned ProductionYieldRecord record.</summary>
public sealed class ProductionYieldRecord : OrganizationEntity
{
    public ProductionYieldRecord(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workOrderId,
        decimal inputQuantity,
        decimal goodOutputQuantity,
        decimal scrapQuantity,
        DateOnly recordedOn)
        : base(id, organizationId, createdAtUtc)
    {
        WorkOrderId = DomainRules.Id(workOrderId, nameof(workOrderId));
        InputQuantity = DomainRules.Positive(inputQuantity, nameof(inputQuantity));
        GoodOutputQuantity = DomainRules.Nonnegative(goodOutputQuantity, nameof(goodOutputQuantity));
        ScrapQuantity = DomainRules.Nonnegative(scrapQuantity, nameof(scrapQuantity));
        RecordedOn = DomainRules.Date(recordedOn, nameof(recordedOn));
    }

    public Guid WorkOrderId { get; }
    public decimal InputQuantity { get; }
    public decimal GoodOutputQuantity { get; }
    public decimal ScrapQuantity { get; }
    public DateOnly RecordedOn { get; }
}
