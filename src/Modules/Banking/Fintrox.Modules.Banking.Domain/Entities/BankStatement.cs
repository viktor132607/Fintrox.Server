namespace Fintrox.Modules.Banking.Domain.Entities;

public sealed class BankStatement : OrganizationEntity
{
    public BankStatement(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid bankAccountId,
        string reference,
        DateOnly startsOn,
        DateOnly endsOn,
        decimal openingBalance,
        decimal closingBalance,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        BankAccountId = DomainRules.Id(bankAccountId, nameof(bankAccountId));
        Reference = DomainRules.Text(reference, nameof(reference));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        OpeningBalance = openingBalance;
        ClosingBalance = closingBalance;
        Currency = DomainRules.Currency(currency, nameof(currency));
        if (EndsOn < StartsOn) throw new ArgumentException("The end cannot precede the start.", nameof(endsOn));
    }

    public Guid BankAccountId { get; }
    public string Reference { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public decimal OpeningBalance { get; }
    public decimal ClosingBalance { get; }
    public string Currency { get; }
}
