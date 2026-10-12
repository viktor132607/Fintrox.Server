namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped SupplierQuotationRequest model. References require application validation.</summary>
public sealed class SupplierQuotationRequest : OrganizationEntity
{
    public SupplierQuotationRequest(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid purchaseRequisitionId,
        string requestNumber,
        DateOnly issuedOn,
        DateOnly responseDueOn)
        : base(id, organizationId, createdAtUtc)
    {
        PurchaseRequisitionId = DomainRules.Id(purchaseRequisitionId, nameof(purchaseRequisitionId));
        RequestNumber = DomainRules.Text(requestNumber, nameof(requestNumber));
        IssuedOn = DomainRules.Date(issuedOn, nameof(issuedOn));
        ResponseDueOn = DomainRules.Date(responseDueOn, nameof(responseDueOn));
    }

    public Guid PurchaseRequisitionId { get; }
    public string RequestNumber { get; }
    public DateOnly IssuedOn { get; }
    public DateOnly ResponseDueOn { get; }
}
