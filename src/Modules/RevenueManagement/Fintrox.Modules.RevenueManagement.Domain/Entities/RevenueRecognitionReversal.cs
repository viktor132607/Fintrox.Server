namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

/// <summary>Organization-scoped immutable RevenueRecognitionReversal model; no cross-module navigations.</summary>
public sealed class RevenueRecognitionReversal : OrganizationEntity
{
    public RevenueRecognitionReversal(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid originalRecognitionEventId,
        Guid reversalJournalId,
        DateOnly reversedOn,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        OriginalRecognitionEventId = DomainRules.Id(originalRecognitionEventId, nameof(originalRecognitionEventId));
        ReversalJournalId = DomainRules.Id(reversalJournalId, nameof(reversalJournalId));
        ReversedOn = DomainRules.Date(reversedOn, nameof(reversedOn));
        Amount = DomainRules.Positive(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid OriginalRecognitionEventId { get; }
    public Guid ReversalJournalId { get; }
    public DateOnly ReversedOn { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
