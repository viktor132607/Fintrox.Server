namespace Fintrox.Modules.Integrations.Domain.Entities;

public sealed class AppRegistration : OrganizationEntity
{
    public AppRegistration(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        Guid ownerUserId)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        OwnerUserId = DomainRules.Id(ownerUserId, nameof(ownerUserId));
    }

    public string Code { get; }
    public string Name { get; }
    public Guid OwnerUserId { get; }
}
