namespace Fintrox.Modules.Identity.Domain.Entities;

public sealed class RoleDefinition : OrganizationEntity
{
    public RoleDefinition(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
    }

    public string Code { get; }
    public string Name { get; }
}
