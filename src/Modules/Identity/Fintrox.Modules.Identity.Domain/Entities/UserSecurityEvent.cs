namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned UserSecurityEvent domain record; references checked in application layer.</summary>
public sealed class UserSecurityEvent : OrganizationEntity
{
    public UserSecurityEvent(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        string eventCode,
        string ipAddress,
        DateTimeOffset occurredAtUtc,
        string riskLevel)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        EventCode = DomainRules.Text(eventCode, nameof(eventCode));
        IpAddress = DomainRules.Text(ipAddress, nameof(ipAddress));
        OccurredAtUtc = DomainRules.Instant(occurredAtUtc, nameof(occurredAtUtc));
        RiskLevel = DomainRules.Text(riskLevel, nameof(riskLevel));
    }

    public Guid UserId { get; }
    public string EventCode { get; }
    public string IpAddress { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public string RiskLevel { get; }
}
