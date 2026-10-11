namespace Fintrox.Modules.Organizations.Domain.Entities;

public sealed class Branch : OrganizationEntity
{
    public Branch(
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
