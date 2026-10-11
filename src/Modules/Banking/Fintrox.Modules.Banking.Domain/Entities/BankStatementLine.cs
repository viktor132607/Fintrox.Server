namespace Fintrox.Modules.Banking.Domain.Entities;

public sealed class BankStatementLine : OrganizationEntity
{
    public BankStatementLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid statementId,
        DateOnly bookedOn,
        string reference,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        StatementId = DomainRules.Id(statementId, nameof(statementId));
        BookedOn = DomainRules.Date(bookedOn, nameof(bookedOn));
        Reference = DomainRules.Text(reference, nameof(reference));
        Amount = DomainRules.Nonzero(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid StatementId { get; }
    public DateOnly BookedOn { get; }
    public string Reference { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
