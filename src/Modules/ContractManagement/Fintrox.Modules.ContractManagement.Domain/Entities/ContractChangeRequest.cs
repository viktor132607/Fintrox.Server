namespace Fintrox.Modules.ContractManagement.Domain.Entities;

/// <summary>Tenant-owned ContractChangeRequest; external references validated by application layer.</summary>
public sealed class ContractChangeRequest : OrganizationEntity
{
    public ContractChangeRequest(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid businessContractId,
        Guid requestedByUserId,
        DateTimeOffset requestedAtUtc,
        string reason,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        BusinessContractId = DomainRules.Id(businessContractId, nameof(businessContractId));
        RequestedByUserId = DomainRules.Id(requestedByUserId, nameof(requestedByUserId));
        RequestedAtUtc = DomainRules.Instant(requestedAtUtc, nameof(requestedAtUtc));
        Reason = DomainRules.Text(reason, nameof(reason));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid BusinessContractId { get; }
    public Guid RequestedByUserId { get; }
    public DateTimeOffset RequestedAtUtc { get; }
    public string Reason { get; }
    public string Status { get; }
}
