namespace Fintrox.Modules.Currencies.Domain.Entities;

/// <summary>Tenant-scoped CurrencyRoundingAdjustment. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CurrencyRoundingAdjustment : OrganizationEntity
{
    public CurrencyRoundingAdjustment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid sourceDocumentId,
        decimal originalAmount,
        decimal roundedAmount,
        string currency,
        int precisionScale)
        : base(id, organizationId, createdAtUtc)
    {
        SourceDocumentId = DomainRules.Id(sourceDocumentId, nameof(sourceDocumentId));
        OriginalAmount = DomainRules.Nonzero(originalAmount, nameof(originalAmount));
        RoundedAmount = DomainRules.Nonzero(roundedAmount, nameof(roundedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        PrecisionScale = DomainRules.Scale(precisionScale, nameof(precisionScale));
    }

    public Guid SourceDocumentId { get; }
    public decimal OriginalAmount { get; }
    public decimal RoundedAmount { get; }
    public string Currency { get; }
    public int PrecisionScale { get; }
}
