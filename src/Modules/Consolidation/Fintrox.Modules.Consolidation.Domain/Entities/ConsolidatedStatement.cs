namespace Fintrox.Modules.Consolidation.Domain.Entities;

/// <summary>Tenant-owned ConsolidatedStatement; external references validated by application layer.</summary>
public sealed class ConsolidatedStatement : OrganizationEntity
{
    public ConsolidatedStatement(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid consolidationRunId,
        string statementType,
        string presentationCurrency,
        DateOnly asOfDate)
        : base(id, organizationId, createdAtUtc)
    {
        ConsolidationRunId = DomainRules.Id(consolidationRunId, nameof(consolidationRunId));
        StatementType = DomainRules.Text(statementType, nameof(statementType));
        PresentationCurrency = DomainRules.Currency(presentationCurrency, nameof(presentationCurrency));
        AsOfDate = DomainRules.Date(asOfDate, nameof(asOfDate));
    }

    public Guid ConsolidationRunId { get; }
    public string StatementType { get; }
    public string PresentationCurrency { get; }
    public DateOnly AsOfDate { get; }
}
