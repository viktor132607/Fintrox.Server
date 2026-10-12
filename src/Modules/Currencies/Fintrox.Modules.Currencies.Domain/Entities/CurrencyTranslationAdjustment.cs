namespace Fintrox.Modules.Currencies.Domain.Entities;

/// <summary>Tenant-scoped CurrencyTranslationAdjustment. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CurrencyTranslationAdjustment : OrganizationEntity
{
    public CurrencyTranslationAdjustment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid revaluationRunId,
        Guid sourceDocumentId,
        decimal adjustmentAmount,
        string currency,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        RevaluationRunId = DomainRules.Id(revaluationRunId, nameof(revaluationRunId));
        SourceDocumentId = DomainRules.Id(sourceDocumentId, nameof(sourceDocumentId));
        AdjustmentAmount = DomainRules.Nonzero(adjustmentAmount, nameof(adjustmentAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid RevaluationRunId { get; }
    public Guid SourceDocumentId { get; }
    public decimal AdjustmentAmount { get; }
    public string Currency { get; }
    public string Reason { get; }
}
