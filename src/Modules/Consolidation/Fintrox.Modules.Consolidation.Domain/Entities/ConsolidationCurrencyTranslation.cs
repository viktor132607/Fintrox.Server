namespace Fintrox.Modules.Consolidation.Domain.Entities;

/// <summary>Tenant-owned ConsolidationCurrencyTranslation; external references validated by application layer.</summary>
public sealed class ConsolidationCurrencyTranslation : OrganizationEntity
{
    public ConsolidationCurrencyTranslation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid consolidationRunId,
        Guid sourceCompanyId,
        string sourceCurrency,
        string targetCurrency,
        decimal rate,
        decimal translationDifference)
        : base(id, organizationId, createdAtUtc)
    {
        ConsolidationRunId = DomainRules.Id(consolidationRunId, nameof(consolidationRunId));
        SourceCompanyId = DomainRules.Id(sourceCompanyId, nameof(sourceCompanyId));
        SourceCurrency = DomainRules.Currency(sourceCurrency, nameof(sourceCurrency));
        TargetCurrency = DomainRules.Currency(targetCurrency, nameof(targetCurrency));
        Rate = DomainRules.Positive(rate, nameof(rate));
        TranslationDifference = DomainRules.Nonnegative(translationDifference, nameof(translationDifference));
    }

    public Guid ConsolidationRunId { get; }
    public Guid SourceCompanyId { get; }
    public string SourceCurrency { get; }
    public string TargetCurrency { get; }
    public decimal Rate { get; }
    public decimal TranslationDifference { get; }
}
