namespace Fintrox.Modules.OrderManagement.Domain.Entities;

/// <summary>Immutable, organization-owned OrderPromise record.</summary>
public sealed class OrderPromise : OrganizationEntity
{
    public OrderPromise(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fulfillmentOrderId,
        DateOnly promisedDeliveryOn,
        string promiseSource,
        bool isConfirmed)
        : base(id, organizationId, createdAtUtc)
    {
        FulfillmentOrderId = DomainRules.Id(fulfillmentOrderId, nameof(fulfillmentOrderId));
        PromisedDeliveryOn = DomainRules.Date(promisedDeliveryOn, nameof(promisedDeliveryOn));
        PromiseSource = DomainRules.Text(promiseSource, nameof(promiseSource));
        IsConfirmed = isConfirmed;
    }

    public Guid FulfillmentOrderId { get; }
    public DateOnly PromisedDeliveryOn { get; }
    public string PromiseSource { get; }
    public bool IsConfirmed { get; }
}
