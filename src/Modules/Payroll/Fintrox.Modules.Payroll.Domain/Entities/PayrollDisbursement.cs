namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollDisbursement; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollDisbursement : OrganizationEntity
{
    public PayrollDisbursement(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid paymentInstructionId,
        Guid paymentId,
        DateOnly paidOn,
        decimal paidAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        PaymentInstructionId = DomainRules.Id(paymentInstructionId, nameof(paymentInstructionId));
        PaymentId = DomainRules.Id(paymentId, nameof(paymentId));
        PaidOn = DomainRules.Date(paidOn, nameof(paidOn));
        PaidAmount = DomainRules.Nonnegative(paidAmount, nameof(paidAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid PaymentInstructionId { get; }
    public Guid PaymentId { get; }
    public DateOnly PaidOn { get; }
    public decimal PaidAmount { get; }
    public string Currency { get; }
}
