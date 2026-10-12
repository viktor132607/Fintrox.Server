namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped BankPaymentInstruction record; cross-module references require application authorization.</summary>
public sealed class BankPaymentInstruction : OrganizationEntity
{
    public BankPaymentInstruction(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid bankAccountId,
        Guid beneficiaryId,
        string paymentReference,
        DateOnly valueDate,
        decimal amount,
        string currency,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        BankAccountId = DomainRules.Id(bankAccountId, nameof(bankAccountId));
        BeneficiaryId = DomainRules.Id(beneficiaryId, nameof(beneficiaryId));
        PaymentReference = DomainRules.Text(paymentReference, nameof(paymentReference));
        ValueDate = DomainRules.Date(valueDate, nameof(valueDate));
        Amount = DomainRules.Positive(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid BankAccountId { get; }
    public Guid BeneficiaryId { get; }
    public string PaymentReference { get; }
    public DateOnly ValueDate { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string Status { get; }
}
