namespace Fintrox.Modules.Currencies.Domain.Entities;

/// <summary>Tenant-scoped CurrencyConversionExecution. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CurrencyConversionExecution : OrganizationEntity
{
    public CurrencyConversionExecution(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid quoteId,
        Guid sourceTransactionId,
        DateTimeOffset executedAtUtc,
        decimal appliedRate,
        decimal feeAmount)
        : base(id, organizationId, createdAtUtc)
    {
        QuoteId = DomainRules.Id(quoteId, nameof(quoteId));
        SourceTransactionId = DomainRules.Id(sourceTransactionId, nameof(sourceTransactionId));
        ExecutedAtUtc = DomainRules.Instant(executedAtUtc, nameof(executedAtUtc));
        AppliedRate = DomainRules.Positive(appliedRate, nameof(appliedRate));
        FeeAmount = DomainRules.Nonnegative(feeAmount, nameof(feeAmount));
    }

    public Guid QuoteId { get; }
    public Guid SourceTransactionId { get; }
    public DateTimeOffset ExecutedAtUtc { get; }
    public decimal AppliedRate { get; }
    public decimal FeeAmount { get; }
}
