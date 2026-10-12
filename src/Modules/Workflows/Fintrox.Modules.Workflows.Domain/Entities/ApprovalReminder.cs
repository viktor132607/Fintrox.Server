namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned ApprovalReminder; tenant scope and input fields validated at creation.</summary>
public sealed class ApprovalReminder : OrganizationEntity
{
    public ApprovalReminder(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid approvalStepId,
        Guid recipientUserId,
        DateTimeOffset reminderAtUtc,
        bool isSent)
        : base(id, organizationId, createdAtUtc)
    {
        ApprovalStepId = DomainRules.Id(approvalStepId, nameof(approvalStepId));
        RecipientUserId = DomainRules.Id(recipientUserId, nameof(recipientUserId));
        ReminderAtUtc = DomainRules.Instant(reminderAtUtc, nameof(reminderAtUtc));
        IsSent = isSent;
    }

    public Guid ApprovalStepId { get; }
    public Guid RecipientUserId { get; }
    public DateTimeOffset ReminderAtUtc { get; }
    public bool IsSent { get; }
}
