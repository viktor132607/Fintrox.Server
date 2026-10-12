namespace Fintrox.Modules.QualityManagement.Domain.Entities;

/// <summary>Immutable, organization-scoped QualityDefect model. References require application validation.</summary>
public sealed class QualityDefect : OrganizationEntity
{
    public QualityDefect(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inspectionResultId,
        Guid defectCategoryId,
        string description,
        decimal quantityAffected)
        : base(id, organizationId, createdAtUtc)
    {
        InspectionResultId = DomainRules.Id(inspectionResultId, nameof(inspectionResultId));
        DefectCategoryId = DomainRules.Id(defectCategoryId, nameof(defectCategoryId));
        Description = DomainRules.Text(description, nameof(description));
        QuantityAffected = DomainRules.Positive(quantityAffected, nameof(quantityAffected));
    }

    public Guid InspectionResultId { get; }
    public Guid DefectCategoryId { get; }
    public string Description { get; }
    public decimal QuantityAffected { get; }
}
