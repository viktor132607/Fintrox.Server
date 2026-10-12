namespace Fintrox.Modules.Payments.Domain.Entities;

/// <summary>Immutable tenant-scoped PaymentAllocationSnapshot; cross-module IDs are verified by use cases.</summary>
public sealed class PaymentAllocationSnapshot : OrganizationEntity
{
    public PaymentAllocationSnapshot(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid paymentId,
        Guid sourceDocumentId,
        decimal allocatedAmount,
        string currency,
        DateOnly allocatedOn)
        : base(id, organizationId, createdAtUtc)
    {
        PaymentId = DomainRules.Id(paymentId, nameof(paymentId));
        SourceDocumentId = DomainRules.Id(sourceDocumentId, nameof(sourceDocumentId));
        AllocatedAmount = DomainRules.Positive(allocatedAmount, nameof(allocatedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        AllocatedOn = DomainRules.Date(allocatedOn, nameof(allocatedOn));
    }

    public Guid PaymentId { get; }
    public Guid SourceDocumentId { get; }
    public decimal AllocatedAmount { get; }
    public string Currency { get; }
    public DateOnly AllocatedOn { get; }
}
