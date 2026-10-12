namespace Fintrox.Modules.QualityManagement.Domain.Entities;

/// <summary>Immutable, organization-scoped InspectionSample model. References require application validation.</summary>
public sealed class InspectionSample : OrganizationEntity
{
    public InspectionSample(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inspectionBatchId,
        int sampleNumber,
        string serialReference,
        string sampleStatus)
        : base(id, organizationId, createdAtUtc)
    {
        InspectionBatchId = DomainRules.Id(inspectionBatchId, nameof(inspectionBatchId));
        SampleNumber = DomainRules.PositiveInt(sampleNumber, nameof(sampleNumber));
        SerialReference = DomainRules.Text(serialReference, nameof(serialReference));
        SampleStatus = DomainRules.Text(sampleStatus, nameof(sampleStatus));
    }

    public Guid InspectionBatchId { get; }
    public int SampleNumber { get; }
    public string SerialReference { get; }
    public string SampleStatus { get; }
}
