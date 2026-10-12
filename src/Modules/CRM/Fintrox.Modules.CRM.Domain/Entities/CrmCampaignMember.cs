namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned CrmCampaignMember record. Domain invariants enforced at construction.</summary>
public sealed class CrmCampaignMember : OrganizationEntity
{
    public CrmCampaignMember(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid campaignId,
        Guid counterpartyId,
        DateOnly joinedOn,
        string memberStatus)
        : base(id, organizationId, createdAtUtc)
    {
        CampaignId = DomainRules.Id(campaignId, nameof(campaignId));
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        JoinedOn = DomainRules.Date(joinedOn, nameof(joinedOn));
        MemberStatus = DomainRules.Text(memberStatus, nameof(memberStatus));
    }

    public Guid CampaignId { get; }
    public Guid CounterpartyId { get; }
    public DateOnly JoinedOn { get; }
    public string MemberStatus { get; }
}
