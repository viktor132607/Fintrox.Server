namespace Fintrox.Modules.ContractManagement.Domain.Entities;

/// <summary>Tenant-owned ContractNotification; external references validated by application layer.</summary>
public sealed class ContractNotification : OrganizationEntity
{
    public ContractNotification(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid businessContractId,
        Guid recipientUserId,
        string notificationType,
        DateOnly dueOn,
        bool isSent)
        : base(id, organizationId, createdAtUtc)
    {
        BusinessContractId = DomainRules.Id(businessContractId, nameof(businessContractId));
        RecipientUserId = DomainRules.Id(recipientUserId, nameof(recipientUserId));
        NotificationType = DomainRules.Text(notificationType, nameof(notificationType));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
        IsSent = isSent;
    }

    public Guid BusinessContractId { get; }
    public Guid RecipientUserId { get; }
    public string NotificationType { get; }
    public DateOnly DueOn { get; }
    public bool IsSent { get; }
}
