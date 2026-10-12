namespace Fintrox.Modules.Currencies.Domain.Entities;

/// <summary>Tenant-scoped CurrencyRateBatch. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CurrencyRateBatch : OrganizationEntity
{
    public CurrencyRateBatch(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid providerId,
        DateOnly rateDate,
        string baseCurrency,
        string importReference,
        DateTimeOffset importedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        ProviderId = DomainRules.Id(providerId, nameof(providerId));
        RateDate = DomainRules.Date(rateDate, nameof(rateDate));
        BaseCurrency = DomainRules.Currency(baseCurrency, nameof(baseCurrency));
        ImportReference = DomainRules.Text(importReference, nameof(importReference));
        ImportedAtUtc = DomainRules.Instant(importedAtUtc, nameof(importedAtUtc));
    }

    public Guid ProviderId { get; }
    public DateOnly RateDate { get; }
    public string BaseCurrency { get; }
    public string ImportReference { get; }
    public DateTimeOffset ImportedAtUtc { get; }
}
