namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped OpeningBalanceLine domain record; referenced objects are validated by application use cases.</summary>
public sealed class OpeningBalanceLine : OrganizationEntity
{
    public OpeningBalanceLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid importId,
        Guid accountId,
        decimal debit,
        decimal credit,
        string currency,
        int lineNumber)
        : base(id, organizationId, createdAtUtc)
    {
        ImportId = DomainRules.Id(importId, nameof(importId));
        AccountId = DomainRules.Id(accountId, nameof(accountId));
        Debit = DomainRules.Nonnegative(debit, nameof(debit));
        Credit = DomainRules.Nonnegative(credit, nameof(credit));
        Currency = DomainRules.Currency(currency, nameof(currency));
        LineNumber = DomainRules.PositiveInt(lineNumber, nameof(lineNumber));
        if (Debit == 0m && Credit == 0m) throw new ArgumentException("At least one side must have an amount.");
    }

    public Guid ImportId { get; }
    public Guid AccountId { get; }
    public decimal Debit { get; }
    public decimal Credit { get; }
    public string Currency { get; }
    public int LineNumber { get; }
}
