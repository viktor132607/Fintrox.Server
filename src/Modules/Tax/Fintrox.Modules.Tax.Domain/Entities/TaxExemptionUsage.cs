namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned TaxExemptionUsage; tenant scope and input fields validated at creation.</summary>
public sealed class TaxExemptionUsage : OrganizationEntity
{
    public TaxExemptionUsage(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid exemptionCertificateId,
        Guid sourceDocumentId,
        DateOnly usedOn,
        decimal exemptAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        ExemptionCertificateId = DomainRules.Id(exemptionCertificateId, nameof(exemptionCertificateId));
        SourceDocumentId = DomainRules.Id(sourceDocumentId, nameof(sourceDocumentId));
        UsedOn = DomainRules.Date(usedOn, nameof(usedOn));
        ExemptAmount = DomainRules.Nonnegative(exemptAmount, nameof(exemptAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid ExemptionCertificateId { get; }
    public Guid SourceDocumentId { get; }
    public DateOnly UsedOn { get; }
    public decimal ExemptAmount { get; }
    public string Currency { get; }
}
