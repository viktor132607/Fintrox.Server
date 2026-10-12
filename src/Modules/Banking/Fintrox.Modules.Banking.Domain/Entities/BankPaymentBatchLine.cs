namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped BankPaymentBatchLine record; cross-module references require application authorization.</summary>
public sealed class BankPaymentBatchLine : OrganizationEntity
{
    public BankPaymentBatchLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid batchId,
        Guid paymentInstructionId,
        int lineNumber,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        BatchId = DomainRules.Id(batchId, nameof(batchId));
        PaymentInstructionId = DomainRules.Id(paymentInstructionId, nameof(paymentInstructionId));
        LineNumber = DomainRules.PositiveInt(lineNumber, nameof(lineNumber));
        Amount = DomainRules.Positive(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid BatchId { get; }
    public Guid PaymentInstructionId { get; }
    public int LineNumber { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
