namespace Fintrox.Modules.Consolidation.Domain.Entities;

/// <summary>Tenant-owned ConsolidationWorksheet; external references validated by application layer.</summary>
public sealed class ConsolidationWorksheet : OrganizationEntity
{
    public ConsolidationWorksheet(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid consolidationRunId,
        string worksheetName,
        Guid companyId,
        string presentationCurrency)
        : base(id, organizationId, createdAtUtc)
    {
        ConsolidationRunId = DomainRules.Id(consolidationRunId, nameof(consolidationRunId));
        WorksheetName = DomainRules.Text(worksheetName, nameof(worksheetName));
        CompanyId = DomainRules.Id(companyId, nameof(companyId));
        PresentationCurrency = DomainRules.Currency(presentationCurrency, nameof(presentationCurrency));
    }

    public Guid ConsolidationRunId { get; }
    public string WorksheetName { get; }
    public Guid CompanyId { get; }
    public string PresentationCurrency { get; }
}
