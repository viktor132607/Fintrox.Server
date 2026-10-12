namespace Fintrox.Modules.ContractManagement.Domain.Entities;

/// <summary>Tenant-owned ContractServiceLevel; external references validated by application layer.</summary>
public sealed class ContractServiceLevel : OrganizationEntity
{
    public ContractServiceLevel(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid businessContractId,
        string metricCode,
        decimal targetValue,
        string measurementUnit)
        : base(id, organizationId, createdAtUtc)
    {
        BusinessContractId = DomainRules.Id(businessContractId, nameof(businessContractId));
        MetricCode = DomainRules.Text(metricCode, nameof(metricCode));
        TargetValue = DomainRules.Nonnegative(targetValue, nameof(targetValue));
        MeasurementUnit = DomainRules.Text(measurementUnit, nameof(measurementUnit));
    }

    public Guid BusinessContractId { get; }
    public string MetricCode { get; }
    public decimal TargetValue { get; }
    public string MeasurementUnit { get; }
}
