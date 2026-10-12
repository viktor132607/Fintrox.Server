namespace Fintrox.Modules.Payments.Domain.Entities;

/// <summary>Immutable tenant-scoped PaymentSchedule; cross-module IDs are verified by use cases.</summary>
public sealed class PaymentSchedule : OrganizationEntity
{
    public PaymentSchedule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid sourceDocumentId,
        DateOnly dueOn,
        decimal amount,
        string currency,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        SourceDocumentId = DomainRules.Id(sourceDocumentId, nameof(sourceDocumentId));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
        Amount = DomainRules.Positive(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid SourceDocumentId { get; }
    public DateOnly DueOn { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string Status { get; }
}
