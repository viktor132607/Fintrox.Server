namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned WorkflowNotification; tenant scope and input fields validated at creation.</summary>
public sealed class WorkflowNotification : OrganizationEntity
{
    public WorkflowNotification(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workflowInstanceId,
        Guid recipientUserId,
        string notificationType,
        DateTimeOffset sentAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        WorkflowInstanceId = DomainRules.Id(workflowInstanceId, nameof(workflowInstanceId));
        RecipientUserId = DomainRules.Id(recipientUserId, nameof(recipientUserId));
        NotificationType = DomainRules.Text(notificationType, nameof(notificationType));
        SentAtUtc = DomainRules.Instant(sentAtUtc, nameof(sentAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid WorkflowInstanceId { get; }
    public Guid RecipientUserId { get; }
    public string NotificationType { get; }
    public DateTimeOffset SentAtUtc { get; }
    public string Status { get; }
}
