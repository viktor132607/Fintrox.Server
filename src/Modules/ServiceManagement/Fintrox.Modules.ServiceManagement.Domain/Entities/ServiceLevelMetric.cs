namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

/// <summary>Tenant-scoped ServiceLevelMetric domain record. No cross-module navigation properties.</summary>
public sealed class ServiceLevelMetric : OrganizationEntity
{
    public ServiceLevelMetric(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid serviceLevelAgreementId,
        string metricCode,
        int targetMinutes,
        string measurementType)
        : base(id, organizationId, createdAtUtc)
    {
        ServiceLevelAgreementId = DomainRules.Id(serviceLevelAgreementId, nameof(serviceLevelAgreementId));
        MetricCode = DomainRules.Text(metricCode, nameof(metricCode));
        TargetMinutes = DomainRules.PositiveInt(targetMinutes, nameof(targetMinutes));
        MeasurementType = DomainRules.Text(measurementType, nameof(measurementType));
    }

    public Guid ServiceLevelAgreementId { get; }
    public string MetricCode { get; }
    public int TargetMinutes { get; }
    public string MeasurementType { get; }
}
