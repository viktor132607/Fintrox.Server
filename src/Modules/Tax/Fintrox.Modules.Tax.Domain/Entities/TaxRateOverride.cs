namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned TaxRateOverride; tenant scope and input fields validated at creation.</summary>
public sealed class TaxRateOverride : OrganizationEntity
{
    public TaxRateOverride(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid taxRateId,
        Guid sourceDocumentId,
        decimal overridePercent,
        string reason,
        Guid approvedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        TaxRateId = DomainRules.Id(taxRateId, nameof(taxRateId));
        SourceDocumentId = DomainRules.Id(sourceDocumentId, nameof(sourceDocumentId));
        OverridePercent = DomainRules.Percent(overridePercent, nameof(overridePercent));
        Reason = DomainRules.Text(reason, nameof(reason));
        ApprovedByUserId = DomainRules.Id(approvedByUserId, nameof(approvedByUserId));
    }

    public Guid TaxRateId { get; }
    public Guid SourceDocumentId { get; }
    public decimal OverridePercent { get; }
    public string Reason { get; }
    public Guid ApprovedByUserId { get; }
}
