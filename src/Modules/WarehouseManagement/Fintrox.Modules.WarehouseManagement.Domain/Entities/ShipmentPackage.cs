namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

public sealed class ShipmentPackage : OrganizationEntity
{
    public ShipmentPackage(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid shipmentId,
        string packageNumber,
        decimal weightKg)
        : base(id, organizationId, createdAtUtc)
    {
        ShipmentId = DomainRules.Id(shipmentId, nameof(shipmentId));
        PackageNumber = DomainRules.Text(packageNumber, nameof(packageNumber));
        WeightKg = DomainRules.Positive(weightKg, nameof(weightKg));
    }

    public Guid ShipmentId { get; }
    public string PackageNumber { get; }
    public decimal WeightKg { get; }
}
