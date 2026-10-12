namespace Fintrox.Modules.ContractManagement.Domain.Entities;

/// <summary>Tenant-owned ContractAuditEvent; external references validated by application layer.</summary>
public sealed class ContractAuditEvent : OrganizationEntity
{
    public ContractAuditEvent(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid businessContractId,
        Guid actorId,
        string eventCode,
        DateTimeOffset occurredAtUtc,
        string details)
        : base(id, organizationId, createdAtUtc)
    {
        BusinessContractId = DomainRules.Id(businessContractId, nameof(businessContractId));
        ActorId = DomainRules.Id(actorId, nameof(actorId));
        EventCode = DomainRules.Text(eventCode, nameof(eventCode));
        OccurredAtUtc = DomainRules.Instant(occurredAtUtc, nameof(occurredAtUtc));
        Details = DomainRules.Text(details, nameof(details));
    }

    public Guid BusinessContractId { get; }
    public Guid ActorId { get; }
    public string EventCode { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public string Details { get; }
}
