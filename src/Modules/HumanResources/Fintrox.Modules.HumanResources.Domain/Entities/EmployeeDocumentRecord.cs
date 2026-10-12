namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned EmployeeDocumentRecord domain model.</summary>
public sealed class EmployeeDocumentRecord : OrganizationEntity
{
    public EmployeeDocumentRecord(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        Guid documentId,
        string documentType,
        DateTimeOffset uploadedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        DocumentId = DomainRules.Id(documentId, nameof(documentId));
        DocumentType = DomainRules.Text(documentType, nameof(documentType));
        UploadedAtUtc = DomainRules.Instant(uploadedAtUtc, nameof(uploadedAtUtc));
    }

    public Guid EmployeeId { get; }
    public Guid DocumentId { get; }
    public string DocumentType { get; }
    public DateTimeOffset UploadedAtUtc { get; }
}
