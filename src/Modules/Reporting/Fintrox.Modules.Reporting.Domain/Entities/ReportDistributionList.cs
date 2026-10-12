namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped ReportDistributionList model. References require application validation.</summary>
public sealed class ReportDistributionList : OrganizationEntity
{
    public ReportDistributionList(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        Guid ownerUserId,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        OwnerUserId = DomainRules.Id(ownerUserId, nameof(ownerUserId));
        IsActive = isActive;
    }

    public string Code { get; }
    public string Name { get; }
    public Guid OwnerUserId { get; }
    public bool IsActive { get; }
}
