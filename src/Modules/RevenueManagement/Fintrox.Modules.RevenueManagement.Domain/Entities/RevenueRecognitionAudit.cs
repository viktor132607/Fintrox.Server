namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

/// <summary>Organization-scoped immutable RevenueRecognitionAudit model; no cross-module navigations.</summary>
public sealed class RevenueRecognitionAudit : OrganizationEntity
{
    public RevenueRecognitionAudit(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid revenueContractId,
        string eventCode,
        Guid actorUserId,
        DateTimeOffset occurredAtUtc,
        string description)
        : base(id, organizationId, createdAtUtc)
    {
        RevenueContractId = DomainRules.Id(revenueContractId, nameof(revenueContractId));
        EventCode = DomainRules.Text(eventCode, nameof(eventCode));
        ActorUserId = DomainRules.Id(actorUserId, nameof(actorUserId));
        OccurredAtUtc = DomainRules.Instant(occurredAtUtc, nameof(occurredAtUtc));
        Description = DomainRules.Text(description, nameof(description));
    }

    public Guid RevenueContractId { get; }
    public string EventCode { get; }
    public Guid ActorUserId { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public string Description { get; }
}
