namespace Fintrox.Modules.Consolidation.Domain.Entities;

/// <summary>Tenant-owned ConsolidationJournal; external references validated by application layer.</summary>
public sealed class ConsolidationJournal : OrganizationEntity
{
    public ConsolidationJournal(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid consolidationRunId,
        string journalNumber,
        DateOnly documentDate,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        ConsolidationRunId = DomainRules.Id(consolidationRunId, nameof(consolidationRunId));
        JournalNumber = DomainRules.Text(journalNumber, nameof(journalNumber));
        DocumentDate = DomainRules.Date(documentDate, nameof(documentDate));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid ConsolidationRunId { get; }
    public string JournalNumber { get; }
    public DateOnly DocumentDate { get; }
    public string Status { get; }
}
