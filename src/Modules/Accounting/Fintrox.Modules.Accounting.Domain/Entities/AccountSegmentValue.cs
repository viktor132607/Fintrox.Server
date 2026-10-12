namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped AccountSegmentValue domain record; referenced objects are validated by application use cases.</summary>
public sealed class AccountSegmentValue : OrganizationEntity
{
    public AccountSegmentValue(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid segmentId,
        string code,
        string label,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        SegmentId = DomainRules.Id(segmentId, nameof(segmentId));
        Code = DomainRules.Text(code, nameof(code));
        Label = DomainRules.Text(label, nameof(label));
        IsActive = isActive;
    }

    public Guid SegmentId { get; }
    public string Code { get; }
    public string Label { get; }
    public bool IsActive { get; }
}
