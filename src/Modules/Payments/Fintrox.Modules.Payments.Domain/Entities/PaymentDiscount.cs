namespace Fintrox.Modules.Payments.Domain.Entities;

/// <summary>Immutable tenant-scoped PaymentDiscount; cross-module IDs are verified by use cases.</summary>
public sealed class PaymentDiscount : OrganizationEntity
{
    public PaymentDiscount(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid paymentScheduleId,
        decimal discountPercent,
        DateOnly eligibleUntil,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        PaymentScheduleId = DomainRules.Id(paymentScheduleId, nameof(paymentScheduleId));
        DiscountPercent = DomainRules.Percent(discountPercent, nameof(discountPercent));
        EligibleUntil = DomainRules.Date(eligibleUntil, nameof(eligibleUntil));
        IsActive = isActive;
    }

    public Guid PaymentScheduleId { get; }
    public decimal DiscountPercent { get; }
    public DateOnly EligibleUntil { get; }
    public bool IsActive { get; }
}
