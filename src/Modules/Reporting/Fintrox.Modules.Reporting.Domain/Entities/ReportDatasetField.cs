namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped ReportDatasetField model. References require application validation.</summary>
public sealed class ReportDatasetField : OrganizationEntity
{
    public ReportDatasetField(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid datasetId,
        string fieldCode,
        string dataType,
        bool isSensitive)
        : base(id, organizationId, createdAtUtc)
    {
        DatasetId = DomainRules.Id(datasetId, nameof(datasetId));
        FieldCode = DomainRules.Text(fieldCode, nameof(fieldCode));
        DataType = DomainRules.Text(dataType, nameof(dataType));
        IsSensitive = isSensitive;
    }

    public Guid DatasetId { get; }
    public string FieldCode { get; }
    public string DataType { get; }
    public bool IsSensitive { get; }
}
