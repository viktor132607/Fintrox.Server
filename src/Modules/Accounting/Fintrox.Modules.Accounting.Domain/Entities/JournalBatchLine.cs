namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped JournalBatchLine domain record; referenced objects are validated by application use cases.</summary>
public sealed class JournalBatchLine : OrganizationEntity
{
    public JournalBatchLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid journalBatchId,
        int lineNumber,
        Guid accountId,
        decimal debit,
        decimal credit,
        string currency,
        string description)
        : base(id, organizationId, createdAtUtc)
    {
        JournalBatchId = DomainRules.Id(journalBatchId, nameof(journalBatchId));
        LineNumber = DomainRules.PositiveInt(lineNumber, nameof(lineNumber));
        AccountId = DomainRules.Id(accountId, nameof(accountId));
        Debit = DomainRules.Nonnegative(debit, nameof(debit));
        Credit = DomainRules.Nonnegative(credit, nameof(credit));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Description = DomainRules.Text(description, nameof(description));
        if (Debit == 0m && Credit == 0m) throw new ArgumentException("At least one side must have an amount.");
    }

    public Guid JournalBatchId { get; }
    public int LineNumber { get; }
    public Guid AccountId { get; }
    public decimal Debit { get; }
    public decimal Credit { get; }
    public string Currency { get; }
    public string Description { get; }
}
