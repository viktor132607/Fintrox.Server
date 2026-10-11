namespace Fintrox.Modules.Consolidation.Domain.Entities;

public sealed class CompanyGroup : OrganizationEntity
{
    public CompanyGroup(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string name,
        string reportingCurrency)
        : base(id, organizationId, createdAtUtc)
    {
        Name = DomainRules.Text(name, nameof(name));
        ReportingCurrency = DomainRules.Currency(reportingCurrency, nameof(reportingCurrency));
    }

    public string Name { get; }
    public string ReportingCurrency { get; }
}
