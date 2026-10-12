namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped FinancialStatementRow model. References require application validation.</summary>
public sealed class FinancialStatementRow : OrganizationEntity
{
    public FinancialStatementRow(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid templateId,
        string lineCode,
        string label,
        string formula,
        int sortOrder)
        : base(id, organizationId, createdAtUtc)
    {
        TemplateId = DomainRules.Id(templateId, nameof(templateId));
        LineCode = DomainRules.Text(lineCode, nameof(lineCode));
        Label = DomainRules.Text(label, nameof(label));
        Formula = DomainRules.Text(formula, nameof(formula));
        SortOrder = DomainRules.NonnegativeInt(sortOrder, nameof(sortOrder));
    }

    public Guid TemplateId { get; }
    public string LineCode { get; }
    public string Label { get; }
    public string Formula { get; }
    public int SortOrder { get; }
}
