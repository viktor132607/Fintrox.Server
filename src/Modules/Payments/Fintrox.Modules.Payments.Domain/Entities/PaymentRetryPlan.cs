namespace Fintrox.Modules.Payments.Domain.Entities;

/// <summary>Immutable tenant-scoped PaymentRetryPlan; cross-module IDs are verified by use cases.</summary>
public sealed class PaymentRetryPlan : OrganizationEntity
{
    public PaymentRetryPlan(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid paymentId,
        DateTimeOffset nextRetryAtUtc,
        int maxAttempts,
        int attemptCount)
        : base(id, organizationId, createdAtUtc)
    {
        PaymentId = DomainRules.Id(paymentId, nameof(paymentId));
        NextRetryAtUtc = DomainRules.Instant(nextRetryAtUtc, nameof(nextRetryAtUtc));
        MaxAttempts = DomainRules.PositiveInt(maxAttempts, nameof(maxAttempts));
        AttemptCount = DomainRules.NonnegativeInt(attemptCount, nameof(attemptCount));
    }

    public Guid PaymentId { get; }
    public DateTimeOffset NextRetryAtUtc { get; }
    public int MaxAttempts { get; }
    public int AttemptCount { get; }
}
