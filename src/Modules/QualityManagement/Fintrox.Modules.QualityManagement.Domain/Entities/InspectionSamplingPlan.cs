namespace Fintrox.Modules.QualityManagement.Domain.Entities;

/// <summary>Immutable, organization-scoped InspectionSamplingPlan model. References require application validation.</summary>
public sealed class InspectionSamplingPlan : OrganizationEntity
{
    public InspectionSamplingPlan(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inspectionPlanId,
        string samplingMethod,
        int sampleSize,
        int acceptableDefects)
        : base(id, organizationId, createdAtUtc)
    {
        InspectionPlanId = DomainRules.Id(inspectionPlanId, nameof(inspectionPlanId));
        SamplingMethod = DomainRules.Text(samplingMethod, nameof(samplingMethod));
        SampleSize = DomainRules.PositiveInt(sampleSize, nameof(sampleSize));
        AcceptableDefects = DomainRules.NonnegativeInt(acceptableDefects, nameof(acceptableDefects));
    }

    public Guid InspectionPlanId { get; }
    public string SamplingMethod { get; }
    public int SampleSize { get; }
    public int AcceptableDefects { get; }
}
