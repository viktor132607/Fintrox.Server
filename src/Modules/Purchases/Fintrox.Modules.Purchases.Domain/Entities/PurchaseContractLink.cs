namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped PurchaseContractLink model. References require application validation.</summary>
public sealed class PurchaseContractLink : OrganizationEntity
{
    public PurchaseContractLink(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid purchaseOrderId,
        Guid contractId,
        Guid contractVersionId,
        DateOnly linkedOn)
        : base(id, organizationId, createdAtUtc)
    {
        PurchaseOrderId = DomainRules.Id(purchaseOrderId, nameof(purchaseOrderId));
        ContractId = DomainRules.Id(contractId, nameof(contractId));
        ContractVersionId = DomainRules.Id(contractVersionId, nameof(contractVersionId));
        LinkedOn = DomainRules.Date(linkedOn, nameof(linkedOn));
    }

    public Guid PurchaseOrderId { get; }
    public Guid ContractId { get; }
    public Guid ContractVersionId { get; }
    public DateOnly LinkedOn { get; }
}
