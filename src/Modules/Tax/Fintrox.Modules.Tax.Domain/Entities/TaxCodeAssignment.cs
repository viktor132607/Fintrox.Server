namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned TaxCodeAssignment; tenant scope and input fields validated at creation.</summary>
public sealed class TaxCodeAssignment : OrganizationEntity
{
    public TaxCodeAssignment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid sourceDocumentLineId,
        Guid taxCategoryId,
        Guid taxRateId,
        DateOnly appliedOn)
        : base(id, organizationId, createdAtUtc)
    {
        SourceDocumentLineId = DomainRules.Id(sourceDocumentLineId, nameof(sourceDocumentLineId));
        TaxCategoryId = DomainRules.Id(taxCategoryId, nameof(taxCategoryId));
        TaxRateId = DomainRules.Id(taxRateId, nameof(taxRateId));
        AppliedOn = DomainRules.Date(appliedOn, nameof(appliedOn));
    }

    public Guid SourceDocumentLineId { get; }
    public Guid TaxCategoryId { get; }
    public Guid TaxRateId { get; }
    public DateOnly AppliedOn { get; }
}
