namespace Fintrox.Modules.Audit.Domain.Entities;

/// <summary>Tenant-scoped AuditAccessReview record; cross-module references require application authorization.</summary>
public sealed class AuditAccessReview : OrganizationEntity
{
    public AuditAccessReview(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid reviewerId,
        DateOnly startsOn,
        DateOnly endsOn,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        ReviewerId = DomainRules.Id(reviewerId, nameof(reviewerId));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        Status = DomainRules.Text(status, nameof(status));
        if (EndsOn < StartsOn) throw new ArgumentException("End must not precede start.", nameof(endsOn));
    }

    public Guid ReviewerId { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public string Status { get; }
}
