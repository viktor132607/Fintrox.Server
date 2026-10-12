namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned ApprovalGroup; tenant scope and input fields validated at creation.</summary>
public sealed class ApprovalGroup : OrganizationEntity
{
    public ApprovalGroup(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string groupCode,
        string name,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        GroupCode = DomainRules.Text(groupCode, nameof(groupCode));
        Name = DomainRules.Text(name, nameof(name));
        IsActive = isActive;
    }

    public string GroupCode { get; }
    public string Name { get; }
    public bool IsActive { get; }
}
