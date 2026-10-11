namespace Fintrox.Modules.Manufacturing.Domain.Entities;

public sealed class ProductionOutput : OrganizationEntity
{
    public ProductionOutput(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workOrderId,
        Guid itemId,
        decimal goodQuantity,
        decimal scrapQuantity)
        : base(id, organizationId, createdAtUtc)
    {
        WorkOrderId = DomainRules.Id(workOrderId, nameof(workOrderId));
        ItemId = DomainRules.Id(itemId, nameof(itemId));
        GoodQuantity = DomainRules.Nonnegative(goodQuantity, nameof(goodQuantity));
        ScrapQuantity = DomainRules.Nonnegative(scrapQuantity, nameof(scrapQuantity));
        if (GoodQuantity == 0 && ScrapQuantity == 0) throw new ArgumentException("An output must record a positive quantity.", nameof(goodQuantity));
    }

    public Guid WorkOrderId { get; }
    public Guid ItemId { get; }
    public decimal GoodQuantity { get; }
    public decimal ScrapQuantity { get; }
}
