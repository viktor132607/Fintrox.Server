namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned TaxCalculationContext; tenant scope and input fields validated at creation.</summary>
public sealed class TaxCalculationContext : OrganizationEntity
{
    public TaxCalculationContext(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid sourceDocumentId,
        string documentType,
        DateOnly taxPointOn,
        string supplyCountryCode,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        SourceDocumentId = DomainRules.Id(sourceDocumentId, nameof(sourceDocumentId));
        DocumentType = DomainRules.Text(documentType, nameof(documentType));
        TaxPointOn = DomainRules.Date(taxPointOn, nameof(taxPointOn));
        SupplyCountryCode = DomainRules.Country(supplyCountryCode, nameof(supplyCountryCode));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid SourceDocumentId { get; }
    public string DocumentType { get; }
    public DateOnly TaxPointOn { get; }
    public string SupplyCountryCode { get; }
    public string Currency { get; }
}
