namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

/// <summary>Tenant-scoped ServiceWarrantyClaim domain record. No cross-module navigation properties.</summary>
public sealed class ServiceWarrantyClaim : OrganizationEntity
{
    public ServiceWarrantyClaim(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid serviceEquipmentAssetId,
        Guid supportTicketId,
        DateOnly claimedOn,
        string claimStatus)
        : base(id, organizationId, createdAtUtc)
    {
        ServiceEquipmentAssetId = DomainRules.Id(serviceEquipmentAssetId, nameof(serviceEquipmentAssetId));
        SupportTicketId = DomainRules.Id(supportTicketId, nameof(supportTicketId));
        ClaimedOn = DomainRules.Date(claimedOn, nameof(claimedOn));
        ClaimStatus = DomainRules.Text(claimStatus, nameof(claimStatus));
    }

    public Guid ServiceEquipmentAssetId { get; }
    public Guid SupportTicketId { get; }
    public DateOnly ClaimedOn { get; }
    public string ClaimStatus { get; }
}
