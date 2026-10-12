namespace Fintrox.Modules.Sales.Domain.Entities;

/// <summary>Organization-scoped immutable SalesQuotationRevision model; no cross-module navigations.</summary>
public sealed class SalesQuotationRevision : OrganizationEntity
{
    public SalesQuotationRevision(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid salesQuotationId,
        int revisionNumber,
        DateOnly revisedOn,
        Guid revisedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        SalesQuotationId = DomainRules.Id(salesQuotationId, nameof(salesQuotationId));
        RevisionNumber = DomainRules.PositiveInt(revisionNumber, nameof(revisionNumber));
        RevisedOn = DomainRules.Date(revisedOn, nameof(revisedOn));
        RevisedByUserId = DomainRules.Id(revisedByUserId, nameof(revisedByUserId));
    }

    public Guid SalesQuotationId { get; }
    public int RevisionNumber { get; }
    public DateOnly RevisedOn { get; }
    public Guid RevisedByUserId { get; }
}
