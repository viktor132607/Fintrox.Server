namespace Fintrox.Modules.OrderManagement.Domain.Entities;

/// <summary>Immutable, organization-owned ReturnReason record.</summary>
public sealed class ReturnReason : OrganizationEntity
{
    public ReturnReason(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string description,
        bool requiresInspection,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Description = DomainRules.Text(description, nameof(description));
        RequiresInspection = requiresInspection;
        IsActive = isActive;
    }

    public string Code { get; }
    public string Description { get; }
    public bool RequiresInspection { get; }
    public bool IsActive { get; }
}
