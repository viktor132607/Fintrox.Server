namespace Fintrox.Modules.ContractManagement.Domain.Entities;

/// <summary>Tenant-owned ContractRenewalNotice; external references validated by application layer.</summary>
public sealed class ContractRenewalNotice : OrganizationEntity
{
    public ContractRenewalNotice(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid contractRenewalId,
        DateOnly noticeDate,
        Guid recipientId,
        string noticeMethod,
        DateTimeOffset sentAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        ContractRenewalId = DomainRules.Id(contractRenewalId, nameof(contractRenewalId));
        NoticeDate = DomainRules.Date(noticeDate, nameof(noticeDate));
        RecipientId = DomainRules.Id(recipientId, nameof(recipientId));
        NoticeMethod = DomainRules.Text(noticeMethod, nameof(noticeMethod));
        SentAtUtc = DomainRules.Instant(sentAtUtc, nameof(sentAtUtc));
    }

    public Guid ContractRenewalId { get; }
    public DateOnly NoticeDate { get; }
    public Guid RecipientId { get; }
    public string NoticeMethod { get; }
    public DateTimeOffset SentAtUtc { get; }
}
