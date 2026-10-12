namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollPostingBatch; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollPostingBatch : OrganizationEntity
{
    public PayrollPostingBatch(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid payRunId,
        Guid journalEntryId,
        DateOnly postingDate,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        PayRunId = DomainRules.Id(payRunId, nameof(payRunId));
        JournalEntryId = DomainRules.Id(journalEntryId, nameof(journalEntryId));
        PostingDate = DomainRules.Date(postingDate, nameof(postingDate));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid PayRunId { get; }
    public Guid JournalEntryId { get; }
    public DateOnly PostingDate { get; }
    public string Status { get; }
}
