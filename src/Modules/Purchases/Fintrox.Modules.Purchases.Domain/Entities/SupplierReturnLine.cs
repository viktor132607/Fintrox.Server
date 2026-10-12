namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped SupplierReturnLine model. References require application validation.</summary>
public sealed class SupplierReturnLine : OrganizationEntity
{
    public SupplierReturnLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supplierReturnId,
        Guid inventoryItemId,
        decimal returnedQuantity,
        string returnReason)
        : base(id, organizationId, createdAtUtc)
    {
        SupplierReturnId = DomainRules.Id(supplierReturnId, nameof(supplierReturnId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        ReturnedQuantity = DomainRules.Positive(returnedQuantity, nameof(returnedQuantity));
        ReturnReason = DomainRules.Text(returnReason, nameof(returnReason));
    }

    public Guid SupplierReturnId { get; }
    public Guid InventoryItemId { get; }
    public decimal ReturnedQuantity { get; }
    public string ReturnReason { get; }
}
