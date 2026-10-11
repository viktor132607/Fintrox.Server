namespace Fintrox.Modules.Reporting.Domain.Entities;

public sealed class KpiDefinition : OrganizationEntity
{
    public KpiDefinition(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        string metricCode)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        MetricCode = DomainRules.Text(metricCode, nameof(metricCode));
    }

    public string Code { get; }
    public string Name { get; }
    public string MetricCode { get; }
}
