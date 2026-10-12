namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped JournalApproval domain record; referenced objects are validated by application use cases.</summary>
public sealed class JournalApproval : OrganizationEntity
{
    public JournalApproval(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid journalBatchId,
        Guid approverId,
        string decision,
        DateTimeOffset decidedAtUtc,
        string comment)
        : base(id, organizationId, createdAtUtc)
    {
        JournalBatchId = DomainRules.Id(journalBatchId, nameof(journalBatchId));
        ApproverId = DomainRules.Id(approverId, nameof(approverId));
        Decision = DomainRules.Text(decision, nameof(decision));
        DecidedAtUtc = DomainRules.Instant(decidedAtUtc, nameof(decidedAtUtc));
        Comment = DomainRules.Text(comment, nameof(comment));
    }

    public Guid JournalBatchId { get; }
    public Guid ApproverId { get; }
    public string Decision { get; }
    public DateTimeOffset DecidedAtUtc { get; }
    public string Comment { get; }
}
