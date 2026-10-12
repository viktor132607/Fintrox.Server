namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned ApprovalPolicy; tenant scope and input fields validated at creation.</summary>
public sealed class ApprovalPolicy : OrganizationEntity
{
    public ApprovalPolicy(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string policyCode,
        string name,
        string resourceType,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        PolicyCode = DomainRules.Text(policyCode, nameof(policyCode));
        Name = DomainRules.Text(name, nameof(name));
        ResourceType = DomainRules.Text(resourceType, nameof(resourceType));
        IsActive = isActive;
    }

    public string PolicyCode { get; }
    public string Name { get; }
    public string ResourceType { get; }
    public bool IsActive { get; }
}
