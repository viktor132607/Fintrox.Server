namespace Fintrox.Modules.Audit.Domain.Entities;

/// <summary>Tenant-scoped AuditLegalHold record; cross-module references require application authorization.</summary>
public sealed class AuditLegalHold : OrganizationEntity
{
    public AuditLegalHold(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string matterReference,
        string reason,
        DateOnly startsOn,
        Guid approvedByUserId,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        MatterReference = DomainRules.Text(matterReference, nameof(matterReference));
        Reason = DomainRules.Text(reason, nameof(reason));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        ApprovedByUserId = DomainRules.Id(approvedByUserId, nameof(approvedByUserId));
        IsActive = isActive;
    }

    public string MatterReference { get; }
    public string Reason { get; }
    public DateOnly StartsOn { get; }
    public Guid ApprovedByUserId { get; }
    public bool IsActive { get; }
}
