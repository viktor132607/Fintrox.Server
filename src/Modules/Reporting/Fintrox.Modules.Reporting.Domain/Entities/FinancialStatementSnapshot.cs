namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped FinancialStatementSnapshot model. References require application validation.</summary>
public sealed class FinancialStatementSnapshot : OrganizationEntity
{
    public FinancialStatementSnapshot(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid templateId,
        Guid accountingPeriodId,
        DateTimeOffset generatedAtUtc,
        Guid documentId)
        : base(id, organizationId, createdAtUtc)
    {
        TemplateId = DomainRules.Id(templateId, nameof(templateId));
        AccountingPeriodId = DomainRules.Id(accountingPeriodId, nameof(accountingPeriodId));
        GeneratedAtUtc = DomainRules.Instant(generatedAtUtc, nameof(generatedAtUtc));
        DocumentId = DomainRules.Id(documentId, nameof(documentId));
    }

    public Guid TemplateId { get; }
    public Guid AccountingPeriodId { get; }
    public DateTimeOffset GeneratedAtUtc { get; }
    public Guid DocumentId { get; }
}
