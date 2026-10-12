namespace Fintrox.Modules.Payments.Domain.Entities;

/// <summary>Immutable tenant-scoped PaymentInstallment; cross-module IDs are verified by use cases.</summary>
public sealed class PaymentInstallment : OrganizationEntity
{
    public PaymentInstallment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid paymentScheduleId,
        int installmentNumber,
        DateOnly dueOn,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        PaymentScheduleId = DomainRules.Id(paymentScheduleId, nameof(paymentScheduleId));
        InstallmentNumber = DomainRules.PositiveInt(installmentNumber, nameof(installmentNumber));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
        Amount = DomainRules.Positive(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid PaymentScheduleId { get; }
    public int InstallmentNumber { get; }
    public DateOnly DueOn { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
