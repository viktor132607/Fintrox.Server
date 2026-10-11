namespace Fintrox.Modules.Identity.Domain.Entities;

public sealed class PermissionDefinition : OrganizationEntity
{
    public PermissionDefinition(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string description)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Description = DomainRules.Text(description, nameof(description));
    }

    public string Code { get; }
    public string Description { get; }
}
