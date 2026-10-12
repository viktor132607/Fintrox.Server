namespace Fintrox.Modules.Sales.Domain.Entities;

/// <summary>Organization-scoped immutable SalesOrderAttachment model; no cross-module navigations.</summary>
public sealed class SalesOrderAttachment : OrganizationEntity
{
    public SalesOrderAttachment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid salesOrderId,
        Guid documentId,
        DateTimeOffset uploadedAtUtc,
        string description)
        : base(id, organizationId, createdAtUtc)
    {
        SalesOrderId = DomainRules.Id(salesOrderId, nameof(salesOrderId));
        DocumentId = DomainRules.Id(documentId, nameof(documentId));
        UploadedAtUtc = DomainRules.Instant(uploadedAtUtc, nameof(uploadedAtUtc));
        Description = DomainRules.Text(description, nameof(description));
    }

    public Guid SalesOrderId { get; }
    public Guid DocumentId { get; }
    public DateTimeOffset UploadedAtUtc { get; }
    public string Description { get; }
}
