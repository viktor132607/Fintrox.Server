namespace Fintrox.Modules.Banking.Domain.Entities;

public sealed class BankReconciliationMatch : OrganizationEntity
{
    public BankReconciliationMatch(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid statementLineId,
        Guid paymentId,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        StatementLineId = DomainRules.Id(statementLineId, nameof(statementLineId));
        PaymentId = DomainRules.Id(paymentId, nameof(paymentId));
        Amount = DomainRules.Positive(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid StatementLineId { get; }
    public Guid PaymentId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
