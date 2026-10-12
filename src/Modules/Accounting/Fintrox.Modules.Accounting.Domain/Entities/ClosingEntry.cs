namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped ClosingEntry domain record; referenced objects are validated by application use cases.</summary>
public sealed class ClosingEntry : OrganizationEntity
{
    public ClosingEntry(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid periodCloseRunId,
        Guid journalEntryId,
        Guid accountId,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        PeriodCloseRunId = DomainRules.Id(periodCloseRunId, nameof(periodCloseRunId));
        JournalEntryId = DomainRules.Id(journalEntryId, nameof(journalEntryId));
        AccountId = DomainRules.Id(accountId, nameof(accountId));
        Amount = DomainRules.Nonzero(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid PeriodCloseRunId { get; }
    public Guid JournalEntryId { get; }
    public Guid AccountId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
