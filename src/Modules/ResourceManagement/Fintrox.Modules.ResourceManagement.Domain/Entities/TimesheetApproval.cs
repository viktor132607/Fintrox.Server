namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

/// <summary>Organization-scoped immutable TimesheetApproval model; no cross-module navigations.</summary>
public sealed class TimesheetApproval : OrganizationEntity
{
    public TimesheetApproval(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid timesheetId,
        Guid approverUserId,
        string decision,
        DateTimeOffset decidedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        TimesheetId = DomainRules.Id(timesheetId, nameof(timesheetId));
        ApproverUserId = DomainRules.Id(approverUserId, nameof(approverUserId));
        Decision = DomainRules.Text(decision, nameof(decision));
        DecidedAtUtc = DomainRules.Instant(decidedAtUtc, nameof(decidedAtUtc));
    }

    public Guid TimesheetId { get; }
    public Guid ApproverUserId { get; }
    public string Decision { get; }
    public DateTimeOffset DecidedAtUtc { get; }
}
