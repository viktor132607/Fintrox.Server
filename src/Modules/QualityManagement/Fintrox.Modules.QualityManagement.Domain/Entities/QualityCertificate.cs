namespace Fintrox.Modules.QualityManagement.Domain.Entities;

/// <summary>Immutable, organization-scoped QualityCertificate model. References require application validation.</summary>
public sealed class QualityCertificate : OrganizationEntity
{
    public QualityCertificate(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryLotId,
        string certificateNumber,
        Guid documentId,
        DateOnly issuedOn,
        DateOnly expiresOn)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryLotId = DomainRules.Id(inventoryLotId, nameof(inventoryLotId));
        CertificateNumber = DomainRules.Text(certificateNumber, nameof(certificateNumber));
        DocumentId = DomainRules.Id(documentId, nameof(documentId));
        IssuedOn = DomainRules.Date(issuedOn, nameof(issuedOn));
        ExpiresOn = DomainRules.Date(expiresOn, nameof(expiresOn));
    }

    public Guid InventoryLotId { get; }
    public string CertificateNumber { get; }
    public Guid DocumentId { get; }
    public DateOnly IssuedOn { get; }
    public DateOnly ExpiresOn { get; }
}
