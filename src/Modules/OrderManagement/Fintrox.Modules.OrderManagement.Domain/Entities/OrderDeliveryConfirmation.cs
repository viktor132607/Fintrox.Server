namespace Fintrox.Modules.OrderManagement.Domain.Entities;

/// <summary>Immutable, organization-owned OrderDeliveryConfirmation record.</summary>
public sealed class OrderDeliveryConfirmation : OrganizationEntity
{
    public OrderDeliveryConfirmation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fulfillmentOrderId,
        DateOnly confirmedOn,
        string receiverName,
        Guid proofDocumentId)
        : base(id, organizationId, createdAtUtc)
    {
        FulfillmentOrderId = DomainRules.Id(fulfillmentOrderId, nameof(fulfillmentOrderId));
        ConfirmedOn = DomainRules.Date(confirmedOn, nameof(confirmedOn));
        ReceiverName = DomainRules.Text(receiverName, nameof(receiverName));
        ProofDocumentId = DomainRules.Id(proofDocumentId, nameof(proofDocumentId));
    }

    public Guid FulfillmentOrderId { get; }
    public DateOnly ConfirmedOn { get; }
    public string ReceiverName { get; }
    public Guid ProofDocumentId { get; }
}
