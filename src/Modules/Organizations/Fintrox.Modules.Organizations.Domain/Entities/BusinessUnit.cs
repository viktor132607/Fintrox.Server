namespace Fintrox.Modules.Organizations.Domain.Entities;

public sealed class BusinessUnit : OrganizationEntity
{
    public BusinessUnit(
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
