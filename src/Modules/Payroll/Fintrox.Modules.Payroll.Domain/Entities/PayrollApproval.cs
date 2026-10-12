namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollApproval; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollApproval : OrganizationEntity
{
    public PayrollApproval(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid payRunId,
        Guid approverUserId,
        string decision,
        DateTimeOffset decidedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        PayRunId = DomainRules.Id(payRunId, nameof(payRunId));
        ApproverUserId = DomainRules.Id(approverUserId, nameof(approverUserId));
        Decision = DomainRules.Text(decision, nameof(decision));
        DecidedAtUtc = DomainRules.Instant(decidedAtUtc, nameof(decidedAtUtc));
    }

    public Guid PayRunId { get; }
    public Guid ApproverUserId { get; }
    public string Decision { get; }
    public DateTimeOffset DecidedAtUtc { get; }
}
