namespace Fintrox.Modules.QualityManagement.Domain.Entities;

/// <summary>Immutable, organization-scoped QualityEquipmentCalibration model. References require application validation.</summary>
public sealed class QualityEquipmentCalibration : OrganizationEntity
{
    public QualityEquipmentCalibration(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid equipmentId,
        DateOnly calibratedOn,
        DateOnly nextCalibrationOn,
        Guid certificateDocumentId)
        : base(id, organizationId, createdAtUtc)
    {
        EquipmentId = DomainRules.Id(equipmentId, nameof(equipmentId));
        CalibratedOn = DomainRules.Date(calibratedOn, nameof(calibratedOn));
        NextCalibrationOn = DomainRules.Date(nextCalibrationOn, nameof(nextCalibrationOn));
        CertificateDocumentId = DomainRules.Id(certificateDocumentId, nameof(certificateDocumentId));
    }

    public Guid EquipmentId { get; }
    public DateOnly CalibratedOn { get; }
    public DateOnly NextCalibrationOn { get; }
    public Guid CertificateDocumentId { get; }
}
