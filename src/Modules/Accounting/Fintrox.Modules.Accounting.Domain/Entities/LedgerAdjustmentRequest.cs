namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped LedgerAdjustmentRequest domain record; referenced objects are validated by application use cases.</summary>
public sealed class LedgerAdjustmentRequest : OrganizationEntity
{
    public LedgerAdjustmentRequest(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid accountingBookId,
        Guid sourceJournalId,
        string reason,
        Guid requestedByUserId,
        DateTimeOffset requestedAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        AccountingBookId = DomainRules.Id(accountingBookId, nameof(accountingBookId));
        SourceJournalId = DomainRules.Id(sourceJournalId, nameof(sourceJournalId));
        Reason = DomainRules.Text(reason, nameof(reason));
        RequestedByUserId = DomainRules.Id(requestedByUserId, nameof(requestedByUserId));
        RequestedAtUtc = DomainRules.Instant(requestedAtUtc, nameof(requestedAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid AccountingBookId { get; }
    public Guid SourceJournalId { get; }
    public string Reason { get; }
    public Guid RequestedByUserId { get; }
    public DateTimeOffset RequestedAtUtc { get; }
    public string Status { get; }
}
