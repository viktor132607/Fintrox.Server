namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped FinancialStatementTemplate model. References require application validation.</summary>
public sealed class FinancialStatementTemplate : OrganizationEntity
{
    public FinancialStatementTemplate(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string templateCode,
        string statementType,
        string displayCurrency,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        TemplateCode = DomainRules.Text(templateCode, nameof(templateCode));
        StatementType = DomainRules.Text(statementType, nameof(statementType));
        DisplayCurrency = DomainRules.Currency(displayCurrency, nameof(displayCurrency));
        IsActive = isActive;
    }

    public string TemplateCode { get; }
    public string StatementType { get; }
    public string DisplayCurrency { get; }
    public bool IsActive { get; }
}
